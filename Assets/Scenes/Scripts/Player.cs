using UnityEngine;
using Unity.Netcode;
using Volt.Prediction;

public class Player : PredictedBehaviour<ExampleInputState, ExampleResultState> {
    private CharacterController m_characterController;

    private void Awake() {
        m_characterController = GetComponent<CharacterController>();
    }

    protected override void SetInputs(ref ExampleInputState currentInputs) {
        if (!IsLocalPlayer && NetworkManager.IsConnectedClient)
            return;

        currentInputs.Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        bool cast = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        if (cast)
            currentInputs.Angle = Vector3.SignedAngle(transform.forward, (hit.point - transform.position).normalized, transform.up);
    }

    protected override void SendToClient(ExampleResultState serverResult) {
        SendResults_ClientRpc(serverResult);
    }

    protected override void SendToServer(ExampleInputState clientInput) {
        SendInputs_ServerRpc(clientInput);
    }

    [ServerRpc]
    private void SendInputs_ServerRpc(ExampleInputState clientInput) {
        base.SendToServer(clientInput);
    }

    [ClientRpc]
    private void SendResults_ClientRpc(ExampleResultState serverState) {
        base.SendToClient(serverState);
    }

    protected override bool ErrorCheck(ExampleResultState clientResult, ExampleResultState latestServerResult) {
        Vector3 positionError = clientResult.Position - latestServerResult.Position;

        return positionError.sqrMagnitude > 0.0001f;
    }

    protected override void OnErrorCorrection(ExampleResultState correctResult) {
        transform.position = correctResult.Position;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, correctResult.Angle, transform.eulerAngles.z);
    }

    protected override ExampleResultState ProcessMovement(ExampleInputState input) {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + input.Angle, transform.eulerAngles.z);

        m_characterController.Move(transform.forward * input.Movement * Time.deltaTime * 3);

        return new ExampleResultState {
            PredictionID = input.PredictionID,
            Position = transform.position,
            Angle = transform.rotation.y
        };
    }
}
