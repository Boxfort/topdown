using Godot;

public partial class AttackData : Resource
{
    public int damage;
    public float knockbackForce;
    public Vector2 fromPosition;
    public Node fromNode;

    public AttackData(int damage, float knockbackForce, Vector2 fromPosition, Node fromNode)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.fromPosition = fromPosition;
        this.fromNode = fromNode;
    }
}