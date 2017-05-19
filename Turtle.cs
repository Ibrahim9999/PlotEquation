    public struct Vector3D
    {
    	public double X;
	public double Y;
	public double Z;
	
	public Vector3D(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}
	
	public static Vector3D Zero
	{
		get { return new Vector3D(0, 0, 0); }
	}
	
	public static Vector3D Right
	{
		get { return new Vector3D(1, 0, 0); }
	}
	public static Vector3D Forward
	{
		get { return new Vector3D(0, 1, 0); }
	}
	public static Vector3D Up
	{
		get { return new Vector3D(0, 0, 1); }
	}
	
	public double Magnitude()
	{
		return Math.Sqrt(X*X + Y*Y + Z*Z);
	}
	public static double Magnitude(Vector3D v)
	{
		return Math.Sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z);
	}
	
	public Vector3D Normalize()
	{
		return this / Magnitude();
	}
	public static Vector3D Normalize(Vector3D v)
	{
		return v / Magnitude(v);
	}
	
	public double Angle(Vector3D v)
	{
		return Math.Acos(DotProduct(v) / (Magnitude() * Magnitude(v)));
	}
	
	public override string ToString()
	{
		return(String.Format("({0}, {1}, {2})", X, Y, Z));
	}
	
	public override bool Equals(object obj)
	{
		return this == (Vector3D) obj;
	}
    	
	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}
	
	public static bool operator==(Vector3D a, Vector3D b)
	{
		return a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
	}
	
	public static bool operator!=(Vector3D a, Vector3D b)
	{
		return !(a==b);
	}
	
	public static Vector3D operator+(Vector3D a, Vector3D b)
	{
		return new Vector3D(a.X+b.X, a.Y+b.Y, a.Z+b.Z);
	}
	public static Vector3D operator+(Vector3D a, Quaternion b)
	{
		return new Vector3D(a.X+b.X, a.Y+b.Y, a.Z+b.Z);
	}
	
	public static Vector3D operator-(Vector3D v)
	{
		return v * -1;
	}
	public static Vector3D operator-(Vector3D a, Vector3D b)
	{
		return a + -b;
	}
	
	public static Vector3D operator*(Vector3D v, double d)
	{
		return new Vector3D(v.X*d, v.Y*d, v.Z*d);
	}
	public static Vector3D operator*(Quaternion q, Vector3D v)
	{
		double num = q.X * 2;
		double num2 = q.Y * 2;
		double num3 = q.Z * 2;
		double num4 = q.X * num;
		double num5 = q.Y * num2;
		double num6 = q.Z * num3;
		double num7 = q.X * num2;
		double num8 = q.X * num3;
		double num9 = q.Y * num3;
		double num10 = q.W * num;
		double num11 = q.W * num2;
		double num12 = q.W * num3;
		
		Vector3D result;
		
		result.X = (1 - (num5 + num6)) * v.X + (num7 - num12) * v.Y + (num8 + num11) * v.Z;
		result.Y = (num7 + num12) * v.X + (1  - (num4 + num6)) * v.Y + (num9 - num10) * v.Z;
		result.Z = (num8 - num11) * v.X + (num9 + num10) * v.Y + (1 - (num4 + num5)) * v.Z;
		
		return result;
	}
	public static Vector3D operator*(Vector3D v, Quaternion q)
	{
		return v * q;
	}
	
	public static Vector3D operator/(Vector3D v, double d)
	{
		return v * (1/d);
	}
	
	public double DotProduct(Vector3D v)
	{
		return (X * v.X) + (Y * v.Y) + (Z * v.Z);
	}
	public static double DotProduct(Vector3D a, Vector3D b)
	{
		return a.DotProduct(b);
	}
	
	public Vector3D CrossProduct(Vector3D v)
	{
		return new Vector3D(Y*v.Z - Z*v.Y, Z*v.X - X*v.Z, X*v.Y - Y*v.X);
	}
	public static Vector3D CrossProduct(Vector3D a, Vector3D b)
	{
		return a.CrossProduct(b);
	}
}

public struct Quaternion
{
	public double W;
	public double X;
	public double Y;
	public double Z;
	
	public Quaternion(double w, double x, double y, double z)
	{
		W = w;
		X = x;
		Y = y;
		Z = z;
	}
	public Quaternion(double w, Vector3D v)
	{
		W = w;
		X = v.X;
		Y = v.Y;
		Z = v.Z;
	}
	
	public static Quaternion FromAxisAngle(double angle, Vector3D axis)
	{
		return new Quaternion(Math.Cos(angle/2), axis * Math.Sin(angle/2));
	}
	
	public static Quaternion From2Vectors(Vector3D u, Vector3D v)
	{
//		double m = Math.Sqrt(2 + 2 * Vector3D.DotProduct(u, v));
//		
//		return new Quaternion(.5 * m, Vector3D.CrossProduct(u, v) / m);
		
		
		Quaternion q = new Quaternion(0,Vector3D.CrossProduct(u,v));
		
		if (Equals(q.VectorComponent().Magnitude(), 1)) {
			q.W = 1 + Vector3D.DotProduct(u, v);
		} else {
			q.W = Math.Sqrt(Math.Pow(u.Magnitude(),2) * Math.Pow(v.Magnitude(), 2)) + Vector3D.DotProduct(u, v);
		}
		
		q.Normalize();
		
		return q;
	}
	
	public static Quaternion Identity
	{
		get { return new Quaternion(1, 0, 0, 0); }
	}
	   
	public double Magnitude()
	{
		return Math.Sqrt(W*W + X*X + Y*Y + Z*Z);
	}
	public static double Magnitude(Quaternion q)
	{
		return Math.Sqrt(q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z);
	}
	   
	public Quaternion Conjugate()
	{
		return new Quaternion(W, -X, -Y, -Z);
	}
	public static Quaternion Conjugate(Quaternion q)
	{
		return new Quaternion(q.W, -q.X, -q.Y, -q.Z);
	}
	
	public Quaternion Inverse()
	{
		return this / Math.Pow(Magnitude(), 2);
	}
	public static Quaternion Inverse(Quaternion q)
	{
		return q / Math.Pow(q.Magnitude(), 2);
	}
	
	public Quaternion Normalize()
	{
		return this / Magnitude();
	}
	public static Quaternion Normalize(Quaternion q)
	{
		return q / Magnitude(q);
	}
	
	public Vector3D VectorComponent()
	{
		return new Vector3D(X, Y, Z);
	}
	public static Vector3D VectorComponent(Quaternion q)
	{
		return new Vector3D(q.X, q.Y, q.Z);
	}
	
	public double Angle()
	{
		return 2 * Math.Acos(Normalize().W);
	}
	public static double Angle(Quaternion q)
	{
		return 2 * Math.Acos(q.Normalize().W);
	}
	
	public Vector3D Axis()
	{
		return VectorComponent().Normalize();
	}
	public static Vector3D Axis(Quaternion q)
	{
		return q.VectorComponent().Normalize();
	}
	
	public void ToEuler(ref double yaw, ref double pitch, ref double roll)
	{
		double ysqr = Y * Y;
		
		// pitch (x-axis rotation)
		double t0 = 2 * (W * X + Y * Z);
		double t1 = 1 - 2 * (X * X + ysqr);
		roll = Math.Atan2(t0, t1);
		
		// roll (y-axis rotation)
		t0 = 2 * (W * Y - Z * X);
		t0 = t0 > 1 ? 1 : t0;
		t0 = t0 < -1 ? -1 : t0;
		pitch = Math.Asin(t0);
		
		// yaw (z-axis rotation)
		t0 = 2 * (W * Z + X * Y);
		t1 = 1 - 2 * (ysqr + Z * Z);  
		yaw = Math.Atan2(t0, t1);
	}
	public static void ToEuler(Quaternion q, ref double yaw, ref double pitch, ref double roll)
	{
		double ysqr = q.Y * q.Y;
		
		// pitch (x-axis rotation)
		double t0 = 2 * (q.W * q.X + q.Y * q.Z);
		double t1 = 1 - 2 * (q.X * q.X + ysqr);
		roll = Math.Atan2(t0, t1);
		
		// roll (y-axis rotation)
		t0 = 2 * (q.W * q.Y - q.Z * q.X);
		t0 = t0 > 1 ? 1 : t0;
		t0 = t0 < -1 ? -1 : t0;
		pitch = Math.Asin(t0);
		
		// yaw (z-axis rotation)
		t0 = 2 * (q.W * q.Z + q.X * q.Y);
		t1 = 1 - 2 * (ysqr + q.Z * q.Z);  
		yaw = Math.Atan2(t0, t1);
	}
	
	public override string ToString()
	{
		return(String.Format("({0}, {1}i, {2}j, {3}k)", W, X, Y, Z));
	}
	
	public override bool Equals(object obj)
	{
		return this == (Quaternion) obj;
	}
    	
	public override int GetHashCode()
	{
		return W.GetHashCode() ^ X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}
	
	public static bool operator==(Quaternion a, Quaternion b)
	{
		return a.W.Equals(b.W) && a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
	}
	
	public static bool operator!=(Quaternion a, Quaternion b)
	{
		return !(a==b);
	}
	
	public static Quaternion operator+(Quaternion a, Quaternion b)
	{
		return new Quaternion(a.W+b.W, a.X+b.X, a.Y+b.Y, a.Z+b.Z);
	}
	public static Quaternion operator+(Quaternion a, Vector3D b)
	{
		return new Quaternion(a.W, a.X+b.X, a.Y+b.Y, a.Z+b.Z);
	}
	
	public static Quaternion operator-(Quaternion q)
	{
		return new Quaternion(-q.W, -q.X, -q.Y, -q.Z);
	}
	public static Quaternion operator-(Quaternion a, Quaternion b)
	{
		return a + -b;
	}
	
	public static Quaternion operator*(Quaternion q, double d)
	{
		return new Quaternion(q.W*d, q.X*d, q.Y*d, q.Z*d);
	}
	public static Quaternion operator*(Quaternion a, Quaternion b)
	{
		return new Quaternion(a.W*b.W - a.X*b.X - a.Y*b.Y - a.Z*b.Z,
		                      a.W*b.X + a.X*b.W + a.Y*b.Z - a.Z*b.Y,
		                      a.W*b.Y - a.X*b.Z + a.Y*b.W + a.Z*b.X,
		                      a.W*b.Z + a.X*b.Y - a.Y*b.X + a.Z*b.W);
	}
	
	public static Quaternion operator/(Quaternion q, double d)
	{
		return new Quaternion(q.W/d, q.X/d, q.Y/d, q.Z/d);
	}
}

public class Turtle
{
	public Vector3D forward = new Vector3D(1,0,0);
	public Vector3D position = new Vector3D(0,0,0);
	public Quaternion rotation = Quaternion.Identity;
	
	static Quaternion localRotation = Quaternion.Identity;
	
	public Turtle(Vector3D positionVector, Vector3D forwardVector)
	{
		position = positionVector;
		forward = forwardVector;
	}
	public Turtle() {}
	
	public override string ToString()
	{
		return(String.Format("Position: ({0},{1},{2})  -  ", position.X, position.Y, position.Z) + String.Format("Heading: ({0},{1},{2})", forward.X, forward.Y, forward.Z));
	}
	
	public void MoveForward(double scalar)
	{
		position += forward * scalar;
	}
	
	void TurnForward()
	{
		Vector3D v = localRotation * forward;
		forward = localRotation.Inverse() * v;
	}
	
	public void Yaw(double angle)
	{
		angle *= Math.PI / 180;
		
		Vector3D upAxis = Quaternion.VectorComponent(Quaternion.From2Vectors(Vector3D.Up, forward)).Normalize();
		upAxis = Vector3D.CrossProduct(upAxis, forward);
		
		localRotation = Quaternion.FromAxisAngle(angle / 2, upAxis);
		
		rotation *= localRotation;
		
		TurnForward();
	}
	
	public void Pitch(double angle)
	{
		angle *= Math.PI / 180;
		
		Vector3D rightAxis = Quaternion.VectorComponent(Quaternion.From2Vectors(Vector3D.Forward, forward)).Normalize();
		rightAxis = Vector3D.CrossProduct(rightAxis, forward);
		
		localRotation = Quaternion.FromAxisAngle(angle / 2, rightAxis);
		
		rotation *= localRotation;
		
		TurnForward();
	}
	
	public void Roll(double angle)
	{
		angle *= Math.PI / 180;
		
		localRotation = Quaternion.FromAxisAngle(angle / 2, forward);
		
		rotation *= localRotation;
		
		TurnForward();
	}
}
