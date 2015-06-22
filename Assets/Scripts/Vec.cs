using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Reflection;


namespace Utilities.Geometry
{
    // Vec.cs by roguelikedev on Git (AKA roguelikedev@gmail, AKA roguelikedev on YouTube, etc)
    // do as you will with this code, please show me any improvements you make
    //
    // originally a terse, convenient alternative to XNA Framework's Vector2, Vector3, Point, etc.
    // now updated for Unity
    // benchmarking shows equivalent speed in equivalent operations
    // implicitly casts into most anything one would expect it to
    // serializeable out of the box
    // note that * and / are cross products, not dot products

    // using standard XNA types, let's add a couple of vectors together and upcast the result.
            //Vector2 a = new Vector2(0);
            //Vector2 b = new Vector2(1, 2);
            //Vector2 ab = Vector2.Add(a, b);
            //Vector3 ab3 = new Vector3(ab, 0);
    // now try it with Vec.
            //Vec a = 0;
            //Vec b = Vec.New(1,2);
            //Vec ab3 = a + b;
    // I TOLD YOU IT WAS TERSE

    [Serializable]
    public struct Vec
    {
        #region constants
        public static readonly Vec ONE   =  1;
        public static readonly Vec UP2   = -ONE.UnitY;
        public static readonly Vec DOWN2 =  ONE.UnitY;
        public static readonly Vec UP3   =  ONE.UnitZ;
        public static readonly Vec DOWN3 = -ONE.UnitZ;
        public static readonly Vec LEFT  = -ONE.UnitX;
        public static readonly Vec RIGHT =  ONE.UnitX;
        public static readonly Vec ERROR =  float.MinValue,
                                   FALSE =  ERROR;
        #endregion

        public float x, y, z, d;
        Vec(float x, float y, float z, float d)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.d = d;
        }

        #region factory
        public static Vec New(float x, float y, float z, float d)
        {
            return new Vec(x, y, z, d);
        }
        public static Vec New(float x, float y, float z)
        {
            return new Vec(x, y, z, 0);
        }
        public static Vec New(float x, float y)
        {
            return new Vec(x, y, 0, 0);
        }
        public static Vec New(float all)
        {
            return new Vec(all, all, all, all);
        }
        public static Vec New()
        {
            // note the following two lines are semantically equivalent:
            // Vec v = Vec.New();
            // Vec v = 0;
            return New(0);
        }
        public static Vec Zero { get { return New(0); } }
        #endregion

        #region conversions
//        public static implicit operator Vec(Point p) { return Vec.New(p.X, p.Y); }
        public static implicit operator Vec(float f) { return Vec.New(f); }
        public static implicit operator Vec(Vector2 vec) { return new Vec(vec.x, vec.y, 0, 0); }
        public static implicit operator Vec(Vector3 vec) { return new Vec(vec.x, vec.y, vec.z, 0); }
//        public static implicit operator Vec(Quaternion q) { return new Vec(q.x, q.y, q.z, q.w); }
        public static implicit operator Vec(Color c) { return Vec.New(c.r, c.g, c.b, c.a); }
        public static Vec New(object obj)
        {
            if (obj is Vec) return (Vec)obj;

            if (obj is Vector3)
            {
                return (Vec)(Vector3)obj;
            }
            if (obj is Vector2)
            {
                return (Vec)(Vector2)obj;
            }
//            if (obj is Vector4)
//            {
//                return (Vec)(Vector4)obj;
//            }
            if (obj is Color)
            {
                Color c = (Color)obj;
                return New(c.r, c.g, c.b, c.a);
            }
//            if (obj is Box)
//            {
//                Box box = (Box)obj;
//                //Debug.Assert(box.origin.z == 0);      fixme....4
//                //Debug.Assert(box.dim3.z == 0);
//                return New(box.origin.x, box.origin.y, box.dim3.x, box.dim3.y);
//            }

            throw new InvalidCastException();
        }
        
//        public static implicit operator Quaternion(Vec vec) { return new Quaternion(vec.x, vec.y, vec.z, vec.d); }
        public static implicit operator Vector3(Vec vec) { return new Vector3(vec.x, vec.y, vec.z); }
        public static implicit operator Vector2(Vec vec) { return new Vector2(vec.x, vec.y); }
        public static implicit operator Color(Vec vec) { vec = (vec / 255).Clamp(0, 1); return new Color(vec.x, vec.y, vec.z, vec.d); }
//        public static explicit operator Box(Vec vec) { return Box.New(vec); }
        public Vec To2 { get { return new Vec(x, y, 0, 0); } }
        public Vector3 To3 { get { return new Vector3(x, y, z); } }
        public Quaternion ToQ { get { return new Quaternion(x, y, z, d); } }
//        public Point ToPt
//        {
//            get
//            {
//                return new Point((int)Math.Round(x, MidpointRounding.ToEven)
//                              , (int)Math.Round(y, MidpointRounding.ToEven));
//            }
//        }
        public dynamic Cast(Type to)
        {
//            Debug.Assert(to.IsValueType);
            List<MethodInfo> meth = new List<MethodInfo>(GetType().GetMethods(BindingFlags.Public | BindingFlags.Static));
            meth.RemoveAll(x => x.ReturnType != to);
//            Debug.Assert(meth.Count == 1);
            object[] wasted_keystrokes = { this };
            return meth[0].Invoke(null, wasted_keystrokes);
        }

        /// <summary> returns null if list is null or does not contain 1..4 elements. </summary>
        public static object Parse(string[] arg)
        {
            if (arg == null) return null;
            switch (arg.Length)
            {
                case 0: return null;
                case 1: return Vec.New(float.Parse(arg[0]));
                case 2: return Vec.New(float.Parse(arg[0]), float.Parse(arg[1]));
                case 3: return Vec.New(float.Parse(arg[0]), float.Parse(arg[1]), float.Parse(arg[2]));
                case 4: return Vec.New(float.Parse(arg[0]), float.Parse(arg[1]), float.Parse(arg[2]), float.Parse(arg[3]));
                default: return null;
            }
        }
        public static bool CanRepresent(object obj)
        {
            return obj is Vec || obj is Vector2 || obj is Vector3 || obj is Vector4 || obj is Color;// || obj is Box;
        }

        /// <summary> rval is centered on self. </summary>
//        public Box ToBox(Vec dim3)
//        {
//            return Box.NewAround(this, dim3);
//        }
        #endregion

        #region arithmetic
        public static Vec operator -(Vec self) { return new Vec(-self.x, -self.y, -self.z, -self.d); }
        public static Vec operator -(Vec self, Vec that)
        {
            return New(self.To3 - that.To3);
        }
        /// <summary> divide by zero all day long, nothing will happen. </summary>
        public static Vec operator /(Vec self, Vec that)
        {
			var rval = Vec.New(self.x / that.x, self.y / that.y, self.z / that.z);
            return rval.Take(f => float.IsNaN(f) ? 0 : float.IsInfinity(f) ? 0 : f);
        }
        public static Vec operator /(Vec self, float that)
        {
            return self / New(that);
        }

        public static Vec operator +(Vec self, Vector3 that)
        {
            return New(self + Vec.New(that));
        }
        public static Vec operator +(Vector3 that, Vec self)
        {
            return self + that;
        }
        public static Vec operator +(Vector2 that, Vec self)
        {
            return self.To2 + Vec.New(that);
        }
        public static Vec operator +(Vec self, Vec that)
        {
            return New(self.To3 + that.To3);
        }
        public static Vec operator *(Vec self, Vec that)
        {
			return New(self.x * that.x, self.y * that.y, self.z * that.z);
        }
        public static Vec operator *(Vec self, float that)
        {
            return self * New(that);
        }
        #endregion

        #region comparison
        public static bool operator <=(Vec self, Vec that)
        {
            return self.x <= that.x && self.y <= that.y && self.z <= that.z && self.d <= that.d;
        }
        public static bool operator >=(Vec self, Vec that)
        {
            return self.x >= that.x && self.y >= that.y && self.z >= that.z && self.d >= that.d;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vec)) return false;
            Vec b = (Vec)obj;
            return x == b.x && y == b.y && z == b.z && d == b.d;
        }

        public static bool operator ==(Vec a, Vec b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Vec a, Vec b)
        {
            return !a.Equals(b);
        }

        public static implicit operator bool(Vec v) { return v != ERROR; }
        #endregion

        #region matrix math
        public Vec UnitX { get { Vec rval = 0; rval.x = x; return rval; } }
        public Vec UnitY { get { Vec rval = 0; rval.y = y; return rval; } }
        public Vec UnitZ { get { Vec rval = 0; rval.z = z; return rval; } }

        public static Vec operator |(Vec self, Vec that)
        {
            self.Take(that, (a, b) => (b == 0) ? a : b);
            return self;
        }

        /// <summary> zero stays zero. </summary>
        public Vec Normalish
        {
            get
            {
                if (To3.magnitude == 0) return 0;
                var rval = To3; rval.Normalize(); return rval;
            }
        }
        #endregion

        #region functional functions
        /// <summary> will clobber 'this'.  returns it for convenience only. </summary>
        public Vec Take(Vec that, Func<float, float, float> Comp)
        {
            x = Comp(x, that.x);
            y = Comp(y, that.y);
            z = Comp(z, that.z);
            d = Comp(d, that.d);
            return this;
        }
        /// <summary> will clobber 'this'.  returns it for convenience only. </summary>
        public Vec Take(Func<float, float> Transform)
        {
            x = Transform(x);
            y = Transform(y);
            z = Transform(z);
            d = Transform(d);
            return this;
        }

        public static Vec Zip(Vec a, Vec b, Func<float, float, float> Comp)
        {
            Vec rval = a;
            return rval.Take(b, Comp);
        }
        public static Vec Zip(Vec a, Vec b, Vec c, Func<float, float, float, float> Comp)
        {
            Vec rval = 0;
            rval.x = Comp(a.x, b.x, c.x);
            rval.y = Comp(a.y, b.y, c.y);
            rval.z = Comp(a.z, b.z, c.z);
            rval.d = Comp(a.d, b.d, c.d);
            return rval;
        }


        public Vec Map(Func<float, float> Lambda)
        {
            Vec rval;
            rval.x = Lambda(x);
            rval.y = Lambda(y);
            rval.z = Lambda(z);
            rval.d = Lambda(d);
            return rval;
        }

        /// <summary> returns self without changes.
        /// currently used exclusively for asserts. </summary>
        public Vec Map(Action<float> Lambda)
        {
            Lambda(x);
            Lambda(y);
            Lambda(z);
            Lambda(d);
            return this;
        }

        public static Vec Reduce(List<Vec> ul, Func<float, float, float> Comp)
        {
            Vec rval = ul[0];
            for (int ndx = 0; ++ndx < ul.Count; ) rval.Take(ul[ndx], Comp);
            return rval;
        }

        public float Reduce(float initial, Func<float, float, float> Lambda)
        {
            initial = Lambda(initial, x);
            initial = Lambda(initial, y);
            initial = Lambda(initial, z);
            initial = Lambda(initial, d);
            return initial;
        }
        #endregion

        public float Magnitude
        {
            get
            {
                float rval = 0;
                Map(f => rval += (Single.IsNaN(f)) ? 0 : Math.Abs(f));
                return rval;
            }
        }

        public Vec Clamp(Vec min, Vec max)
        {
            var rval = this;
            rval.Take(min, Math.Max);
            rval.Take(max, Math.Min);
            return rval;
        }

        public float Len3 { get { return To3.magnitude; } }
        public float Len2 { get { return (float)Math.Pow(x * x + y * y, 0.5f); } }

        public float Distance(Vec that)
        {
            return (To3 - that.To3).magnitude;
        }

        #region don't cares
        public override string ToString()
        {
            return string.Format("{0:0.##} {1:0.##} {2:0.##} {3:0.##}", x, y, z, d);
        }

        public override int GetHashCode()
        {
            return To3.GetHashCode();
        }
        #endregion
    }
}