﻿namespace OctopusReleaseRetention.Interfaces;

public interface IRepository<T> where T : class
{
    List<T> GetAll();
    T? GetById(string id);
    void Add(T entity);
    void AddRange(IEnumerable<T> range);
}
