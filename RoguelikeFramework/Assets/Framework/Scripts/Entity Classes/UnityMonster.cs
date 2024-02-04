using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMonster : MonoBehaviour
{
    public SpriteRenderer renderer;

    public RogueHandle<Monster> monsterHandle;
    public RogueHandle<Inventory> inventoryHandle;
    public RogueHandle<Equipment> equipmentHandle;
    // Start is called before the first frame update
    void Start()
    {
        monsterHandle = RogueHandle<Monster>.Default;
        inventoryHandle = RogueHandle<Inventory>.Default;
        equipmentHandle = RogueHandle<Equipment>.Default;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
