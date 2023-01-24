using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Block : MonoBehaviour
{
    public int Value;
    public Block MergingBlock;
    public BlockStateEnum State;
    
    [SerializeField] private SpriteRenderer graphics;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private Node node;

    private void Start()
    {
        State = BlockStateEnum.Idle;
    }

    public Vector2 Position => transform.position;
    public Node Node
    {
        get => node;
        set
        {
            if (node != null)
            {
                node.OccupiedBlock = null;
            }
            node = value;
            node.OccupiedBlock = this;
        }
    }

    public void Init(BlockType type)
    {
        Value = type.Value;
        graphics.color = type.Color;
        text.text = type.Value.ToString();
    }

    // Prepare to merge other block into this
    public void PrepareMerge(Block other)
    {
        // Set blocks' state to merging
        MergingBlock = other;
        State = BlockStateEnum.Merging;
        other.State = BlockStateEnum.ToBeMerged;

        // Set node as unoccupied to allow movement
        node.OccupiedBlock = null;
    }

    public bool CanMerge(int otherValue) => Value == otherValue && State == BlockStateEnum.Idle;
}