using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

//TODO: Add in the controls to move up and down the current stack.

public class ExamineController : MonoBehaviour
{
    public GameObject shortHover;
    public TextMeshProUGUI shortHoverTitle;
    public TextMeshProUGUI shortHoverDesc;

    public EventSystem eventSystem;
    GraphicRaycaster graphicRaycaster;
    Camera camera;

    List<GameObject> examineTargets;
    int index;

    public bool isExamining;
    
    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = transform.parent.GetComponent<GraphicRaycaster>();
        shortHover.SetActive(false);
    }

    public void Toggle()
    {
        isExamining = !isExamining;
    }

    // Update is called once per frame
    void Update()
    {
        if (isExamining)
        {
            List<GameObject> targets = FindValidObjectsForExamine();
            if (targets.Count > 0)
            {
                if (index >= targets.Count)
                {
                    index = targets.Count - 1;
                }
                shortHover.SetActive(true);

                GameObject target = targets[index];
                ExamineDescription desc = target.GetComponent<ExamineDescription>();
                Monster monster = target.GetComponent<Monster>();
                RogueTile tile = target.GetComponent<RogueTile>();
                Item item = target.GetComponent<Item>();
                if (desc)
                {
                    shortHoverTitle.text = "";
                    shortHoverDesc.text = "";
                    if (!desc.locName.IsEmpty)
                    {
                        shortHoverTitle.text = desc.locName.GetLocalizedString();
                    }
                    if (!desc.locDescription.IsEmpty)
                    {
                        shortHoverDesc.text = desc.locDescription.GetLocalizedString();
                    }
                }
                else if (monster)
                {
                    shortHoverTitle.text = monster.localName.GetLocalizedString();
                    shortHoverDesc.text = monster.localDescription.GetLocalizedString();
                }
                else if (item)
                {
                    shortHoverTitle.text = item.GetNameClean();
                    shortHoverDesc.text = "";// item.GetNameClean();
                }
                else if (tile)
                {
                    shortHoverTitle.text = tile.localName.GetLocalizedString();
                    shortHoverDesc.text = tile.localDescription.GetLocalizedString();
                }
            }
            else
            {
                shortHover.SetActive(false);
            }
        }
        else if (shortHover.activeSelf)
        {
            index = 0;
            shortHover.SetActive(false);
        }
    }

    List<GameObject> FindValidObjectsForExamine()
    {
        List<GameObject> objects = new List<GameObject>();
        
        { //Raycast UI
            //Set up the new Pointer Event
            PointerEventData m_PointerEventData = new PointerEventData(eventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            graphicRaycaster.Raycast(m_PointerEventData, results);

            //Find first gameobject with a description
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<ExamineDescription>())
                {
                    objects.Add(result.gameObject);
                    break;
                }
            }
        }

        {
            if (!camera)
            {
                camera = Camera.current;
                if (!camera) return objects;
            }
            //Check for game items!
            Vector3 mouseLoc = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gameLoc = new Vector2Int(Mathf.RoundToInt(mouseLoc.x), Mathf.RoundToInt(mouseLoc.y));
            if (Map.current != null && Map.current.ValidLocation(gameLoc))
            {
                RogueTile tile = Map.current.GetTile(gameLoc);
                if (tile.isVisible && tile.currentlyStanding)
                {
                    objects.Add(tile.currentlyStanding.gameObject);
                }

                if (!tile.isHidden && tile.inventory.Count > 0)
                {
                    foreach (ItemStack stack in tile.inventory.items)
                    {
                        if (stack != null)
                        {
                            objects.Add(stack.held[0].gameObject);
                        }
                    }
                }

                if (!tile.isHidden)
                {
                    objects.Add(tile.gameObject);
                }
            }
        }

        return objects;
    }
}
