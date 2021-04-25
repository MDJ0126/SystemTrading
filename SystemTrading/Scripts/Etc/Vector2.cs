public struct Vector2
{
    public static Vector2 one = new Vector2(1f, 1f);
    public static Vector2 zero = new Vector2(0f, 0f);

    public float x;
    public float y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}
