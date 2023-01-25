using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    public UIManager uiManager;

    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;

    Ray _ray;
    RaycastHit _raycastHit;

    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) _isDraggingMouseBox = false;

        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
        {
            SelectUnitsInDraggingBox();
        }

        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) DeselectAllUnits();

            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _raycastHit, 1000f))
                {
                    if (_raycastHit.transform.tag == "Terrain") DeselectAllUnits();
                }
            }
        }

        if (Input.anyKeyDown)
        {
            int alphaKey = Utils.GetAlphaKeyValue(Input.inputString);
            if (alphaKey != -1)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                    CreateSelectionGroup(alphaKey);
                else
                    ReselectGroup(alphaKey);
            }
            Debug.Log(alphaKey);
        }
    }

    private void SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            _dragStartPosition,
            Input.mousePosition
            );
        
        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");

        bool inBounds;
        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(
                Camera.main.WorldToViewportPoint(unit.transform.position)
            );

            if (inBounds) unit.GetComponent<UnitManager>().Select();
            else unit.GetComponent<UnitManager>().Deselect();
        }
    }

    private void OnGUI()
    {
        if (_isDraggingMouseBox)
        {
            var rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);

            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }

    private void DeselectAllUnits()
    {
        List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (UnitManager unitManager in selectedUnits)
        {
            unitManager.Deselect();
        }
    }

    public void SelectUnitsGroup(int index)
    {
        ReselectGroup(index);
    }

    private void CreateSelectionGroup(int index)
    {
        // check there are units currently selected
        if (Globals.SELECTED_UNITS.Count == 0)
        {
            if (_selectionGroups.ContainsKey(index))
                RemoveSelectionGroup(index);
            return;
        }

        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        _selectionGroups[index] = groupUnits;

        uiManager.ToggleSelectionGroupButton(index, true);
    }

    private void RemoveSelectionGroup(int index)
    {
        _selectionGroups.Remove(index);
        uiManager.ToggleSelectionGroupButton(index, false);
    }

    private void ReselectGroup(int index)
    {
        // check the group actually is defined
        if (!_selectionGroups.ContainsKey(index))
            return;

        DeselectAllUnits();

        foreach (UnitManager unitManager in _selectionGroups[index])
            unitManager.Select();
    }
}
