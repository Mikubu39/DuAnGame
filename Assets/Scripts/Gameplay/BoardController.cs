using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance { get; private set; }

    private List<WoodPlank> _planks;
    private bool _levelDone = false;
    public int prize;

    private void Awake()
    {
        // Đã sửa lỗi tự sát của Level mới
        Instance = this;
    }

    private void Start()
    {
        _planks = FindObjectsOfType<WoodPlank>().ToList();
        Debug.Log($"[Board] Tìm thấy {_planks.Count} ván gỗ trong level này.");
        prize = _planks.Count * 10;
    }

    public void CheckWinCondition()
    {
        if (_levelDone) return;
        if (_planks == null || _planks.Count == 0) return;

        if (_planks.All(p => p == null || !p.gameObject.activeSelf || p.IsFreed))
        {
            _levelDone = true;
            StartCoroutine(TriggerWin());
        }
    }

    private IEnumerator TriggerWin()
    {
        yield return new WaitForSeconds(0.8f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }
}