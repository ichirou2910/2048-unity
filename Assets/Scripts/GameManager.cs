using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Events;
using G2048.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardRenderer;
    [SerializeField] private float blockMovingTime;
    [SerializeField] private float movingThreshold;
    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;

    [SerializeField] private List<BlockType> blockTypes;

    private List<Node> _nodes;
    private List<Block> _blocks;

    private GameStateEnum _gameState;

    private Vector2 mousePos = new(-1, -1);
    private bool isMoving;

#if UNITY_EDITOR
    private const int FINAL_BLOCK = 8;
#else
    private const int FINAL_BLOCK = 2048;
#endif

    void Start()
    {
        _nodes = new();
        _blocks = new();

        ChangeState(GameStateEnum.InitBoard);
    }

    void Update()
    {
        if (_gameState == GameStateEnum.PlayerInput && !isMoving)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 currentMousePos = Input.mousePosition;
                if (mousePos.x < 0)
                    mousePos = currentMousePos;
                if (currentMousePos != mousePos)
                {
                    float distance = Vector2.Distance(currentMousePos, mousePos);
                    if (distance >= movingThreshold)
                    {
                        isMoving = true;
                        Vector2 finalDirection = Vector2.zero;
                        float diffX = currentMousePos.x - mousePos.x;
                        float diffY = currentMousePos.y - mousePos.y;
                        if (Mathf.Abs(diffX) >= Mathf.Abs(diffY))
                        {
                            finalDirection.x = diffX;
                        }
                        else
                        {
                            finalDirection.y = diffY;
                        }

                        finalDirection.Normalize();
                        ShiftBlocks(finalDirection);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mousePos.x = -1;
            isMoving = false;
        }
    }

    private void ChangeState(GameStateEnum newState)
    {
        _gameState = newState;

        switch (newState)
        {
            case GameStateEnum.InitBoard:
                InitializeBoard();
                SpawnRandomBlocks(2);
                break;
            case GameStateEnum.SpawningBlocks:
                SpawnRandomBlocks(1);
                break;
            case GameStateEnum.PlayerInput:
                break;
            case GameStateEnum.MovingBlocks:
                break;
            case GameStateEnum.Win:
            {
                _winScreen.SetActive(true);
                break;
            }
            case GameStateEnum.Lose:
            {
                _loseScreen.SetActive(true);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    void InitializeBoard()
    {
        // Initialize nodes
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }
        }

        var center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);

        // Initialize board
        var board = Instantiate(boardRenderer, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        // Set the camera to the center of the board
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
    }

    void SpawnRandomBlocks(int amount)
    {
        var freeNodes = _nodes.Where(x => x.OccupiedBlock == null).OrderBy(o => Random.value).ToList();
        var nextSpawnValue = Random.value < 0.8f ? 2 : 4;

        foreach (var fNode in freeNodes.Take(amount))
            SpawnBlockAt(fNode, nextSpawnValue);

        if (freeNodes.Count() <= 1)
        {
            ChangeState(GameStateEnum.Lose);
        }
        else
        {
            ChangeState(GameStateEnum.PlayerInput);
        }
    }

    void SpawnBlockAt(Node node, int value)
    {
        var block = Instantiate(blockPrefab, node.Position, Quaternion.identity);
        block.Init(GetBlockType(value));
        block.Node = node;
        _blocks.Add(block);
    }

    void ShiftBlocks(Vector2 direction)
    {
        ChangeState(GameStateEnum.MovingBlocks);

        var blocksTraversal = BuildBlocksTraversal(direction);

        var sequence = DOTween.Sequence();
        bool boardStateChanged = false;

        foreach (var block in blocksTraversal)
        {
            var currentPos = block.Node.Position;
            var nextNode = block.Node;

            do
            {
                block.Node = nextNode;
                var nextPossibleNode = GetNodeAt(nextNode.Position + direction);
                if (nextPossibleNode != null)
                {
                    if (nextPossibleNode.OccupiedBlock == null)
                    {
                        nextNode = nextPossibleNode;
                    }
                    else if (nextPossibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        boardStateChanged = true;
                        block.PrepareMerge(nextPossibleNode.OccupiedBlock);
                    }
                }
            } while (nextNode != block.Node);

            var destination = block.MergingBlock ? block.MergingBlock.Node.Position : block.Node.Position;
            if (destination != currentPos)
                boardStateChanged = true;

            sequence.Insert(0, block.transform.DOMove(destination, blockMovingTime));
        }

        // Player must make a move that change the state of board before the board spawning new block
        if (!boardStateChanged)
        {
            ChangeState(GameStateEnum.PlayerInput);
            return;
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in blocksTraversal.Where(x => x.State == BlockStateEnum.Merging))
            {
                MergeBlocks(block.MergingBlock, block);
            }

            // If 2048 block exists
            if (_blocks.Any(x => x.Value == FINAL_BLOCK))
                ChangeState(GameStateEnum.Win);
            else
                ChangeState(GameStateEnum.SpawningBlocks);
        });
    }

    private void MergeBlocks(Block baseBlock, Block otherBlock)
    {
        SpawnBlockAt(baseBlock.Node, baseBlock.Value * 2);
        RemoveBlock(baseBlock);
        RemoveBlock(otherBlock);
        this.PublishEvent(EventID.OnPlayerScore, baseBlock.Value * 2);
    }

    List<Block> BuildBlocksTraversal(Vector2 direction)
    {
        var order = direction == Vector2.right
            ? _blocks.OrderByDescending(x => x.Position.x)
            : _blocks.OrderBy(x => x.Position.x);

        order = direction == Vector2.up
            ? order.ThenByDescending(x => x.Position.y)
            : order.ThenBy(x => x.Position.y);

        return order.ToList();
    }

    BlockType GetBlockType(int value) => blockTypes.First(x => x.Value == value);

    Node GetNodeAt(Vector2 position) => _nodes.FirstOrDefault(x => x.Position == position);

    void RemoveBlock(Block block)
    {
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }
}