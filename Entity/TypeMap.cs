using System;

namespace Material.Data.Entity
{
    internal struct TypeMap : IEquatable<TypeMap>
    {
        public static TypeMap Create<TParent, TChild>() => new TypeMap(typeof(TParent), typeof(TChild));

        public TypeMap(Type parentType, Type childType)
        {
            ParentType = parentType;
            ChildType = childType;
        }

        public Type ParentType { get; }

        public Type ChildType { get; }

        public override int GetHashCode()
        {
            return ParentType.GetHashCode() ^ ChildType.GetHashCode();
        }

        public bool Equals(TypeMap other)
        {
            return ParentType == other.ParentType && ChildType == other.ChildType;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeMap))
            {
                return false;
            }

            var typeMap = (TypeMap)obj;
            return ParentType == typeMap.ParentType && ChildType == typeMap.ChildType;
        }
    }
}
