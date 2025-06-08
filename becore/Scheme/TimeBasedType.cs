namespace becore.Scheme;

public abstract class TimeBasedType<T>
{
    public Guid Id { get; set; }
    public required T Value { get; set; }
    public Guid? ParentId { get; set; }
    public TimeBasedType<T>? Parent { get; set; }
    public Guid? ChildId { get; set; }
    public TimeBasedType<T>? Child { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;
}

public class TimeBasedString : TimeBasedType<string> { }
public class TimeBasedInt : TimeBasedType<int> { }
public class TimeBasedBool : TimeBasedType<bool> { }
public class TimeBasedByte : TimeBasedType<byte> { }
public class TimeBasedDouble : TimeBasedType<double> { }
