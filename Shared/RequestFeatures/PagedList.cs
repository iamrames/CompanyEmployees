﻿namespace Shared.RequestFeatures;
using Microsoft.EntityFrameworkCore;

public class PagedList<T>: List<T>
{
    public MetaData MetaData { get; set; }
    public PagedList(List<T> items, int count, int pageNumber, int pageSize) 
    { 
        MetaData = new MetaData 
        { 
            TotalCount = count, 
            PageSize = pageSize, 
            CurrentPage = pageNumber, 
            TotalPages = (int)Math.Ceiling(count / (double)pageSize) 
        };
        AddRange(items); 
    }
    public async static Task<PagedList<T>> ToPagedListAsync(IOrderedQueryable<T> source, int pageNumber, int pageSize)
    { 
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); 
        return new PagedList<T>(items, count, pageNumber, pageSize); 
    }
}
