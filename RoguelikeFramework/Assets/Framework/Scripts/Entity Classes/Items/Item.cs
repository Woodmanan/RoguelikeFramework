using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour, IDescribable
{
    [Header("Generation attributes")]
    public ItemRarity rarity;
    public ItemType type;
    public int minDepth;
    public int maxDepth;
    public ItemRarity elevatesTo;

    [HideInInspector] public ItemRarity currentRarity;

    [Header("Basic item variables")]
    public int ID;
    public bool stackable;
    

    [SerializeField] Color color;

    [HideInInspector] public Vector2Int location;
    public bool held;
    private Monster heldBy;
    [SerializeField] public string friendlyName;
    [SerializeField] public LocalizedString localName;
    [SerializeField] public LocalizedString localDescription;

    [HideInInspector] public bool CanEquip;
    [HideInInspector] public bool CanActivate;
    [HideInInspector] public bool CanMelee;
    [HideInInspector] public bool CanRanged;

    public Connections connections;
    [HideInInspector] public List<Effect> attachedEffects = new List<Effect>();

    [SerializeReference] public List<Effect> baseEffects;


    [SerializeReference] public List<Effect> optionalEffects;


    private SpriteRenderer Render;
    public SpriteRenderer render
    {
        get 
        {
            if (Render)
            {
                return Render;
            }
            else
            {
                Render = GetComponent<SpriteRenderer>();
                Render.sortingOrder = -900;
                return Render;
            }
        }

        set
        {
            Render = value;
        }
    }

    [HideInInspector] public ActivatableItem activatable;
    [HideInInspector] public EquipableItem equipable;
    [HideInInspector] public MeleeWeapon melee;
    [HideInInspector] public RangedWeapon ranged;

    private static readonly float itemZValue = -7.0f;

    private bool setup = false;

    //Stuff used for convenience editor hacking, and should never be seen.

    /* If you see this and don't know what this is, ask me! It's super useful
     * for hacking up the editor, and making things easy. The #if's in this file
     * are used to make the sprite in the sprite renderer equal the sprite in this file,
     * so you can't forget to not change both. */
    #if UNITY_EDITOR
    private Color currentColor;
    #endif

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    public Item Instantiate()
    {
        Item i = Instantiate(gameObject).GetComponent<Item>();
        return i;
    }

    public void Setup()
    {
        if (setup) return;
        if (this.type == 0)
        {
            Debug.LogError("An item is set to have no type! Please use ItemType.MISC if you have misc items.", this);
        }

        activatable = GetComponent<ActivatableItem>();
        equipable = GetComponent<EquipableItem>();
        melee = GetComponent<MeleeWeapon>();
        ranged = GetComponent<RangedWeapon>();

        //Quick check for components, better here than later
        CanEquip = (equipable != null);
        CanActivate = (activatable != null);
        CanMelee = (melee != null);
        CanRanged = (ranged != null);

        //Set up connections before attaching default effects
        if (connections == null) connections = new Connections(this);

        AddEffect(baseEffects.Select(x => x.Instantiate()).ToArray());
        currentRarity = rarity;
        setup = true;
    }

    public string GetName(bool shorten = false)
    {
        string name = "";
        if (melee && melee.enchantment > 0)
        {
            name = $"+{melee.enchantment} ";
        }
        else if (ranged && ranged.enchantment > 0)
        {
            name = $"+{ranged.enchantment} ";
        }

        name += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(currentRarity))}>" + localName.GetLocalizedString(shorten);

        foreach (Effect effect in attachedEffects)
        {
            if (effect.ShouldDisplay())
            {
                name += $" {{{effect.GetName(shorten)}}}";
            }
        }

        if (equipable)
        {
            foreach (Effect effect in equipable.addedEffects)
            {
                if (effect.ShouldDisplay())
                {
                    name += $" {{{effect.GetName(shorten)}}}";
                }
            }
        }

        if (CanEquip && equipable.isEquipped)
        {
            name += " <color=#74AE93> [Equipped]";
        }

        return name;
    }

    public string GetDescription()
    {
        return localDescription.GetLocalizedString(this);
    }

    public Sprite GetImage()
    {
        return render.sprite;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pickup(Monster m)
    {
        heldBy = m;
        DisableSprite();
    }

    public void Drop()
    {
        heldBy = null;
    }

    public void SetLocation(Vector2Int loc)
    {
        this.location = loc;
        transform.position = new Vector3(loc.x, loc.y, itemZValue);
    }

    public void EnableSprite()
    {
        render.enabled = true;
    }

    public void DisableSprite()
    {
        render.enabled = false;
    }

    public void SetGrayscale()
    {
        float gray = color.grayscale;
        render.color = new Color(gray, gray, gray);
    }

    public void SetFullColor()
    {
        render.color = color;
    }

    public virtual void Apply()
    {

    }

    public virtual void RegenerateStats()
    {

    }

    //Returns the name without modifiers. As of right now, just returns the straight name.
    public string GetNameClean()
    {
        return localName.GetLocalizedString();
    }

    public string GetPlural()
    {
        return localName.GetLocalizedString();
    }

    //TODO: Items should elevate stats as well
    public void ElevateRarityTo(ItemRarity rarity, List<Effect> elevationOptions = null)
    {
        int numberToAdd = rarity - this.rarity;

        if (elevationOptions != null)
        {
            optionalEffects.AddRange(elevationOptions);
        }

        if (optionalEffects.Count < numberToAdd)
        {
            Debug.LogWarning($"{friendlyName} does not have enough options to elevate fully to rarity {rarity}! Please add more options, or mark its achievable rarity correctly.");
        }

        for (int i = 0; i < System.Math.Min(numberToAdd, optionalEffects.Count); i++)
        {
            int index = UnityEngine.Random.Range(0, optionalEffects.Count);
            AddEffect(optionalEffects[index].Instantiate());
            optionalEffects.RemoveAt(index);
            this.rarity++;
        }

        if (melee != null || ranged != null)
        {
            Weapon weapon = (melee != null) ? (Weapon) melee : (Weapon) ranged;
            if (weapon.CanAddEnchantment())
            {
                weapon.AddEnchantment(Mathf.RoundToInt(RogueRNG.BoundedParetoCut(1, numberToAdd + RogueRNG.Linear(0, numberToAdd), 1, 2)));
            }
        }

        currentRarity = rarity;
    }

    public void AddEffect(params Effect[] effects)
    {
        foreach (Effect e in effects)
        {
            e.Connect(connections);
            attachedEffects.Add(e);
        }
    }

    //Editor only functions - For convenience
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (render.color != currentColor)
        {
            currentColor = render.color;
            color = render.color;
        }
        else if (color != currentColor)
        {
            currentColor = color;
            render.color = color;
        }
    }
    #endif

    public static Color GetRarityColor(ItemRarity rarity)
    {
        Color outColor = Color.black;
        switch (rarity)
        {
            case ItemRarity.COMMON:
                ColorUtility.TryParseHtmlString("#ABB2BF", out outColor);
                break;
            case ItemRarity.UNCOMMON:
                ColorUtility.TryParseHtmlString("#98C379", out outColor);
                break;
            case ItemRarity.RARE:
                ColorUtility.TryParseHtmlString("#E06C75", out outColor);
                break;
            case ItemRarity.EPIC:
                ColorUtility.TryParseHtmlString("#C678DD", out outColor);
                break;
            case ItemRarity.LEGENDARY:
                ColorUtility.TryParseHtmlString("#E5C07B", out outColor);
                break;
            case ItemRarity.UNIQUE:
                ColorUtility.TryParseHtmlString("#56B6C2", out outColor);
                break;
        }

        return outColor;
    }
}
