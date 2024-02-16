using Unity.Mathematics;
using UnityEngine;

#nullable enable

/// <summary>
/// Simple camera controls to allow following the BubbleSort.
/// </summary>
internal sealed class CameraController : MonoBehaviour
{
    private Controls _controls;
    private float _moveSpeed = 5.0f;

    /// <summary>
    /// Preliminary setup.
    /// </summary>
    private void Start()
    {
        _controls = new Controls();
        _controls.Enable();
    }

    /// <summary>
    /// Adjust camera position on the x axis and z axis based on user input.
    /// </summary>
    private void Update()
    {
        _moveSpeed += _controls.CameraMovement.CameraSpeed.ReadValue<float>() / 50.0f;
        _moveSpeed = math.clamp(_moveSpeed, 0.0f, float.MaxValue);

        // Even though this errors in Visual Studio, this works perfectly fine in Unity.
        transform.localPosition = transform.localPosition
            with
            {
                x = math.clamp(transform.localPosition.x + _controls.CameraMovement.Camera.ReadValue<float>() * Time.deltaTime * _moveSpeed, 0, BubbleSortVisualization.s_lastElementPositionX),
                z = math.clamp(transform.localPosition.z + _controls.CameraMovement.CameraZoom.ReadValue<float>() * Time.deltaTime * _moveSpeed , -2000.0f, -25)
            };
    }
}
