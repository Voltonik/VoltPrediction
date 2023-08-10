using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

namespace Volt.Prediction {
    public abstract class PredictedBehaviour<TInput, TResult> : NetworkBehaviour
            where TInput : IBaseNetworkInputState, new()
            where TResult : IBaseNetworkResultState, new() {

        // Shared
        public int BufferSize = 512;

        // Client
        private TResult[] m_stateBuffer;
        private TInput[] m_inputBuffer;
        private TInput m_currentInput;
        private TResult m_latestServerState;

        // Server
        public float ServerSampleRate;
        private float m_minTimeBetweenTicks;
        private int m_currentPredictionID;
        private float m_timer;
        private TResult[] m_serverStateBuffer;
        protected Queue<TInput> m_inputQueue = new();

        public virtual void Start() {
            m_minTimeBetweenTicks = 1 / ServerSampleRate;

            m_stateBuffer = new TResult[BufferSize];
            m_inputBuffer = new TInput[BufferSize];

            m_serverStateBuffer = new TResult[BufferSize];

            if (IsServer)
                StartCoroutine(ServerTicker());
        }

        private IEnumerator ServerTicker() {
            while (true) {
                m_timer += Time.deltaTime;

                while (m_timer >= m_minTimeBetweenTicks) {
                    m_timer -= m_minTimeBetweenTicks;

                    HandleServerTick();
                }

                yield return null;
            }
        }

        public virtual void Update() {
            if (IsServer)
                return;

            SetInputs(ref m_currentInput);
        }

        public virtual void FixedUpdate() {
            if (IsServer)
                return;

            HandleClientTick();
        }

        private void HandleServerTick() {
            int bufferIndex = -1;

            while (m_inputQueue.Count > 0) {
                m_currentInput = m_inputQueue.Dequeue();

                bufferIndex = m_currentInput.PredictionID % BufferSize;

                m_serverStateBuffer[bufferIndex] = ProcessMovement(m_currentInput);
            }

            if (bufferIndex != -1)
                SendToClient(m_serverStateBuffer[bufferIndex]);
        }

        private void HandleClientTick() {
            int bufferIndex = m_currentPredictionID % BufferSize;

            TInput inputPayload = m_currentInput;
            inputPayload.PredictionID = m_currentPredictionID;
            m_inputBuffer[bufferIndex] = inputPayload;

            m_stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

            if (NetworkManager.IsConnectedClient)
                SendToServer(inputPayload);

            m_currentPredictionID++;
        }

        private void HandleServerReconciliation() {
            int bufferIndex = m_latestServerState.PredictionID % BufferSize;

            if (ErrorCheck(m_stateBuffer[bufferIndex], m_latestServerState)) {
                OnErrorCorrection(m_latestServerState);

                for (int replayedPredictionID = m_latestServerState.PredictionID + 1; replayedPredictionID < m_currentPredictionID; ++replayedPredictionID) {
                    bufferIndex = replayedPredictionID % BufferSize;

                    m_stateBuffer[bufferIndex] = ProcessMovement(m_inputBuffer[bufferIndex]);
                }
            }
        }

        protected virtual void SendToServer(TInput clientInput) {
            m_inputQueue.Enqueue(clientInput);
        }

        protected virtual void SendToClient(TResult serverResult) {
            m_latestServerState = serverResult;

            HandleServerReconciliation();
        }

        protected virtual bool ErrorCheck(TResult clientResult, TResult latestServerResult) {
            Vector3 positionError = clientResult.Position - latestServerResult.Position;

            return positionError.sqrMagnitude > 0.0001f;
        }

        protected virtual void OnErrorCorrection(TResult correctResult) {
            transform.position = correctResult.Position;
        }

        protected virtual void SetInputs(ref TInput currentInputs) { }

        protected virtual TResult ProcessMovement(TInput input) => default;
    }
}