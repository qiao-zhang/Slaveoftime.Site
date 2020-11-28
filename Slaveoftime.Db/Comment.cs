namespace Slaveoftime.Db;

using System;
using System.Collections.Generic;

public class Comment
{
    public Guid Id { get; set; }

    public string Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedTime { get; set; }

    public Guid PostId { get; set; }
    public Post Post { get; set; }

    public Guid? ParentId { get; set; }
    public Comment Parent { get; set; }

    public ICollection<Comment> Children { get; set; }
}
