using KnightOfNights.Scripts.SharedLib;
using System;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
internal class HeroDetectorProxy : MonoBehaviour
{
    private event Action? OnDetectedEvent;
    private event Action? OnUndetectedEvent;

    private int detections = 0;
    private bool prevDetected = false;

    public bool Detected() => detections > 0;

    private void OnTriggerEnter2D(Collider2D collider) => ++detections;

    private void OnTriggerExit2D(Collider2D collider) => --detections;

    private void OnDisable()
    {
        if (!prevDetected) return;

        detections = 0;
        Update();
    }

    public void OnDetected(Action action)
    {
        if (Detected()) action();
        OnDetectedEvent += action;
    }

    public void Listen(Action detect, Action undetect)
    {
        if (Detected()) detect.Invoke();

        OnDetectedEvent += detect;
        OnUndetectedEvent += undetect;
    }

    private void Update()
    {
        bool newDetected = Detected();
        if (newDetected != prevDetected)
        {
            if (newDetected) OnDetectedEvent?.Invoke();
            else OnUndetectedEvent?.Invoke();

            prevDetected = newDetected;
        }
    }
}
