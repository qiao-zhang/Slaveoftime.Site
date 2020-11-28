namespace Slaveoftime.Db;

using System;
using System.Collections.Generic;

public class Post
{
    public Guid Id { get; set; }

    public string Title { get; set; }
    public string Keywords { get; set; }
    public string Description { get; set; }
    public int ViewCount { get; set; }
    public string Author { get; set; }
    public bool IsActive { get; set; }

    public DateTime UpdatedTime { get; set; }
    public DateTime CreatedTime { get; set; }

    public ICollection<Comment> Comments { get; set; }
}
