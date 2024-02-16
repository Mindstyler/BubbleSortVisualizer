using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

#nullable enable

/// <summary>
/// Spawns colored cubes to visualize the BubbleSort sorting algorithm.
/// </summary>
internal sealed class BubbleSortVisualization : MonoBehaviour
{
    [SerializeField]
    private UIDocument _ui;
    [SerializeField]
    private SplineContainer _swapSpline1;
    [SerializeField]
    private SplineContainer _swapSpline2;
    [SerializeField]
    private SplineContainer _noSwapSpline1;
    [SerializeField]
    private SplineContainer _noSwapSpline2;

    internal static float s_lastElementPositionX;

    private static float s_speed;
    private static GameObject[] s_visualizationContainer = Array.Empty<GameObject>();
    private static int[]? s_containerToSort;
    private static bool s_paused = true;
#pragma warning disable CS8618 // This pragma can be removed when MemberNotNullAttribute becomes available
    private static IEnumerator<object?> s_bubbleSortEnumerator;
#pragma warning restore CS8618

    private static Spline s_swapSpline1;
    private static Spline s_swapSpline2;
    private static Spline s_noSwapSpline1;
    private static Spline s_noSwapSpline2;

    private static Transform? s_item1transform;
    private static Transform? s_item2transform;
    private static float3 s_originalItem1Position;
    private static float3 s_originalItem2Position;
    private static float s_splineTime;
    private static bool s_visualSwap;

    private static readonly System.Random s_random = new();
    private static readonly quaternion s_270DegreeRotation = quaternion.EulerZXY(math.radians(270.0f), 0.0f, 0.0f);

    /// <summary>
    /// Preliminary setup.
    /// </summary>
    private void Start()
    {
        SetSplines();
        Initialize();
    }

    /// <summary>
    /// Animates the latest operated-on cubes along respective splines.
    /// </summary>
    private void Update()
    {
        if (s_item1transform == null)
        {
            return;
        }

        s_item1transform.localPosition = s_originalItem1Position + math.rotate(s_270DegreeRotation, SplineUtility.EvaluatePosition(s_visualSwap ? s_swapSpline1 : s_noSwapSpline1, s_splineTime));
        s_item2transform!.localPosition = s_originalItem2Position + math.rotate(s_270DegreeRotation, SplineUtility.EvaluatePosition(s_visualSwap ? s_swapSpline2 : s_noSwapSpline2, s_splineTime));

        if (s_splineTime >= 1.0f)
        {
            s_splineTime = 0.0f;
            s_item1transform = null;
            s_item2transform = null;

            if (!s_paused)
            {
                s_bubbleSortEnumerator.MoveNext();
            }

            return;
        }

        s_splineTime += Time.deltaTime / s_speed;
    }

#if UNITY_EDITOR
#pragma warning disable IDE0051
    /// <summary>
    /// Resets static fields and properties for <see href="https://docs.unity3d.com/Manual/ConfigurableEnterPlayMode.html">Enter Play Mode Options</see>.
    /// </summary>
    [InitializeOnEnterPlayMode]
    private static void EditorReset()
    {
        s_splineTime = 0.0f;
        s_paused = true;
        s_visualizationContainer = Array.Empty<GameObject>();
        s_containerToSort = null;
        s_lastElementPositionX = 0.0f;
        s_speed = 0.0f;
        s_visualizationContainer = Array.Empty<GameObject>();
        s_containerToSort = null;
        s_paused = true;
        s_bubbleSortEnumerator = null!;
        s_swapSpline1 = null!;
        s_swapSpline2 = null!;
        s_noSwapSpline1 = null!;
        s_noSwapSpline2 = null!;
        s_item1transform = null;
        s_item2transform = null;
        s_originalItem1Position = Vector3.zero;
        s_originalItem2Position = Vector3.zero;
        s_splineTime = 0.0f;
        s_visualSwap = false;
}
#pragma warning restore IDE0051
#endif

    /// <summary>
    /// Copies the relevant spline data from the SplineContainer helper objects.
    /// </summary>
    private void SetSplines()
    {
        s_swapSpline1 = _swapSpline1.Spline;
        s_swapSpline2 = _swapSpline2.Spline;
        s_noSwapSpline1 = _noSwapSpline1.Spline;
        s_noSwapSpline2 = _noSwapSpline2.Spline;

        Destroy(_swapSpline1.gameObject);
        Destroy(_swapSpline2.gameObject);
        Destroy(_noSwapSpline1.gameObject);
        Destroy(_noSwapSpline2.gameObject);
    }

    /// <summary>
    /// BubbleSort algorithm, slightly adapted for visualization purposes.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}" /> for sorting step by step.</returns>
    private static IEnumerator<object?> BubbleSort()
    {
        bool swapped;

        for (int i = 0; i < s_containerToSort!.Length - 1; ++i)
        {
            swapped = false;

            for (int j = 0; j < s_containerToSort.Length - i - 1; ++j)
            {
                s_item1transform = s_visualizationContainer[j].transform;
                s_item2transform = s_visualizationContainer[j + 1].transform;

                s_originalItem1Position = s_item1transform.localPosition;
                s_originalItem2Position = s_item2transform.localPosition;

                if (s_containerToSort[j] > s_containerToSort[j + 1])
                {
                    // Using value tuples to swap elements is a lot more concise than old-style swapping and gets lowered to the same code anyway.
                    (s_containerToSort[j], s_containerToSort[j + 1]) = (s_containerToSort[j + 1], s_containerToSort[j]);
                    swapped = true;

                    (s_visualizationContainer[j], s_visualizationContainer[j + 1]) = (s_visualizationContainer[j + 1], s_visualizationContainer[j]);
                    s_visualSwap = true;
                }
                else
                {
                    s_visualSwap = false;
                }

                yield return null;
            }

            if (!swapped)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Initialize necessary UI elements.
    /// </summary>
    private void Initialize()
    {
        // If more logic would need to be added here or to the event handlers, extracting parts into their own methods could be appropriate.
        UnsignedIntegerField itemCountInput = _ui.rootVisualElement.Q<UnsignedIntegerField>("item-count-input");
        itemCountInput.RegisterValueChangedCallback(changeEvent =>
        {
            if (changeEvent.newValue == changeEvent.previousValue)
            {
                return;
            }

            if (changeEvent.newValue > 10_000)
            {
                ResetAll(10_000);
                itemCountInput.SetValueWithoutNotify(10_000);
                return;
            }

            if (changeEvent.newValue < 2)
            {
                ResetAll(2);
                itemCountInput.SetValueWithoutNotify(2);
                return;
            }

            ResetAll(changeEvent.newValue);

            s_paused = true;
        });

        SliderInt operationsPerSecond = _ui.rootVisualElement.Q<SliderInt>("operations-per-second-slider");
        operationsPerSecond.RegisterValueChangedCallback(static changeEvent => s_speed = 1.0f / changeEvent.newValue);

        s_speed = 1.0f / operationsPerSecond.value;

        Button playToggle = _ui.rootVisualElement.Q<Button>("play-toggle");
        playToggle.clicked += () =>
        {
            s_paused = !s_paused;
            playToggle.text = s_paused ? "Play" : "Pause";

            if (!s_paused)
            {
                s_bubbleSortEnumerator.MoveNext();
            }
        };

        _ui.rootVisualElement.Q<Button>("step-button").clicked += () =>
        {
            s_paused = true;
            playToggle.text = "Play";

            if (s_splineTime == 0.0f)
            {
                s_bubbleSortEnumerator.MoveNext();
            }
        };

        _ui.rootVisualElement.Q<Button>("reset-button").clicked += () =>
        {
            s_paused = true;

            ResetAll(itemCountInput.value);
        };

        if (s_containerToSort is null)
        {
            ResetAll(itemCountInput.value);
        }
    }

    /// <summary>
    /// Reset and repopulate 
    /// </summary>
    /// <param name="arraySize">Size of the arrays.</param>
    //[MemberNotNull(nameof(s_bubbleSortEnumerator))] // only available in .net 6+
    private static void ResetAll(uint arraySize)
    {
        s_containerToSort = new int[arraySize];

        foreach (GameObject item in s_visualizationContainer)
        {
            // Instead of destroying the GameObjects, using an IObjectPool could be one possible optimization if one were to look at
            // multiple BubbleSorts with a lot of elements. This happening is unlikely however;
            Destroy(item);
        }

        s_visualizationContainer = new GameObject[s_containerToSort.Length];

        for (int i = 0; i < s_containerToSort.Length; ++i)
        {
            s_containerToSort[i] = i;
        }

        // This shuffle can be made more expressive by using Random.Shuffle() once .net 8 becomes available.
        s_containerToSort = s_containerToSort.OrderBy(static _ => s_random.Next()).ToArray();

        float stepValue = 1.0f / (s_containerToSort.Length - 1);

        for (int i = 0; i < s_containerToSort.Length; ++i)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.Lerp(Color.red, Color.green, stepValue * s_containerToSort[i]));
            item.transform.localPosition = new Vector3(i + (i * 3), 0, 0);

            s_visualizationContainer[i] = item;
        }

        s_lastElementPositionX = s_visualizationContainer[^1].transform.position.x;

        s_bubbleSortEnumerator = BubbleSort();
    }
}
