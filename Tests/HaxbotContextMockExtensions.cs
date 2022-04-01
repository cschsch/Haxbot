using Haxbot;
using Haxbot.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests;

public static class HaxbotContextMockExtensions
{
    public static Mock<HaxbotContext> Add<TEntity>(this Mock<HaxbotContext> context, ICollection<TEntity> values)
        where TEntity : Entity
    {
        var data = values.AsQueryable();
        var set = new Mock<DbSet<TEntity>>();
        set.As<IQueryable<TEntity>>().Setup(s => s.Provider).Returns(data.Provider);
        set.As<IQueryable<TEntity>>().Setup(s => s.Expression).Returns(data.Expression);
        set.As<IQueryable<TEntity>>().Setup(s => s.ElementType).Returns(data.ElementType);
        set.As<IQueryable<TEntity>>().Setup(s => s.GetEnumerator()).Returns(data.GetEnumerator());

        switch (typeof(TEntity))
        {
            case var player when typeof(TEntity) == typeof(Player): context.Setup(ctx => ctx.Players).Returns(set.Object as DbSet<Player>); break;
            case var teams when typeof(TEntity) == typeof(Team): context.Setup(ctx => ctx.Teams).Returns(set.Object as DbSet<Team>); break;
            case var games when typeof(TEntity) == typeof(Game): context.Setup(ctx => ctx.Games).Returns(set.Object as DbSet<Game>); break;
            default: throw new ArgumentException($"Type {typeof(TEntity).Name} not recognized!");
        }

        context.Setup(s => s.Add(It.IsAny<TEntity>())).Callback<TEntity>(value => values.Add(value));

        return context;
    }
}
