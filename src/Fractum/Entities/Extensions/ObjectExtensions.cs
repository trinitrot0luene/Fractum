﻿namespace Fractum.Entities.Extensions
{
    public static class ObjectExtensions
    {
        public static T Cast<T>(this object obj)
            => (T) obj;
    }
}