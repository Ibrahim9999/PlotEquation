public struct Complex
{
	public double R;
	public double I;
	
	public Complex(double real, double imaginary)
	{
		R = real;
		I = imaginary;
	}
	
	public double Real
	{
		get { return R; }
		
		set { R = 1; }
	}
	
	public double Imaginary
	{
		get { return I; }
		
		set { I = 1; }
	}
	
	public double Radius
	{
		get { return Abs(new Complex(R,I)); }
	}
	
	public double Theta
	{
		get { return Arg(new Complex(R,I)); }
	}
	
	public override string ToString()
	{
		return(String.Format("({0}, {1}i)", R, I));
	}
	
	public override bool Equals(object obj)
	{
		return this == (Complex) obj;
	}
	
	public override int GetHashCode()
	{
		return R.GetHashCode() ^ I.GetHashCode();
	}
	
	public static bool operator==(Complex z, Complex d)
	{
		return z.R.Equals(d.R) && z.I.Equals(d.I);
	}
	
	public static bool operator!=(Complex z, Complex d)
	{
		return !(z == d);
	}
	
	public static Complex operator+(Complex z, Complex d)
	{
		return new Complex(z.R + d.R, z.I + d.I);
	}
	public static Complex operator+(Complex z, int d)
	{
		return z + Convert.ToDouble(d);
	}
	public static Complex operator+(int d, Complex z)
	{
		return z + Convert.ToDouble(d);
	}
	public static Complex operator+(Complex z, double d)
	{
		return d + z;
	}
	public static Complex operator+(double d, Complex z)
	{
		return new Complex(z.R + d, z.I);
	}
	
	public static Complex operator-(Complex z)
	{
		return z * -1;
	}
	public static Complex operator-(Complex z, double d)
	{
		return new Complex(z.R - d, z.I - d);
	}
	public static Complex operator-(double d, Complex z)
	{
		return new Complex(d - z.R, d - z.I);
	}
	public static Complex operator-(Complex z, Complex d)
	{
		return new Complex(z.R - d.R, z.I - d.I);
	}
	
	public static Complex operator*(Complex z, int d)
	{
		return z * Convert.ToDouble(d);
	}
	public static Complex operator*(int d, Complex z)
	{
		return z * Convert.ToDouble(d);
	}
	public static Complex operator*(Complex z, double d)
	{
		return new Complex(z.R * d, z.I * d);
	}
	public static Complex operator*(double d, Complex z)
	{
		return new Complex(z.R * d, z.I * d);
	}
	public static Complex operator*(Complex z, Complex d)
	{
		return new Complex(z.R * d.R - z.I * d.I, z.R * d.I + d.R * z.I);
	}
	
	public static Complex operator/(Complex z, double d)
	{
		if (d.Equals(0))
			//throw new DivideByZeroException("Can't divide by zero Complex number");
			return new Complex(0,0);
		
		return new Complex(z.R / d, z.I / d);
	}
	public static Complex operator/(double d, Complex z)
	{
		if (d.Equals(0))
			//throw new DivideByZeroException("Can't divide by zero Complex number");
			return new Complex(0,0);
		
		return new Complex(d / z.R, d / z.I);
	}
	public static Complex operator/(Complex z, Complex d)
	{
		if (d == new Complex(0,0))
			//throw new DivideByZeroException("Can't divide by zero Complex number");
			return d;
		
		return new Complex((z.R * d.R + z.I * d.I) / (d.R * d.R + d.I * d.I), (d.R * z.I - z.R * d.I) /
		(d.R * d.R + d.I * d.I));
	}
	
	public static Complex operator^(Complex z, double p)
	{
		if (p.Equals(1))
			return z;
		if (p > 0 && p.Equals((int) p))
		{
			Complex nz = z;
			
			for (int i = 0; i < p - 1; i++)
				nz = nz*z;
			
			return nz;
		}
		
		double r = Math.Pow(Abs(z), p),
			   theta = Arg(z);
		
		return Double.IsNaN(theta) || Double.IsNaN(r) ? z : new Complex(r * Math.Cos(theta * p), r * Math.Sin(theta * p));
	}
	public static Complex operator^(double d, Complex z)
	{
		double r = Math.Pow(d, z.R),
			   theta = z.I * Math.Log(d);
		
		return Double.IsNaN(theta) || Double.IsNaN(r) ? z : new Complex(r * Math.Cos(theta), r * Math.Sin(theta));
	}
	public static Complex operator^(Complex z, Complex p)
	{
		return ((z.R*z.R + z.I*z.I) ^ (p / 2)) * Exp((new Complex(0,1)) * p * Arg(z));
	}
	
	public static double Arg(Complex z)
	{
		double arg = Math.Atan2(z.I, z.R);
		
		/* 
		 * 	Atan2 (four-quadrant arctangent):
		 * 		
		 * 		double atan = Math.Atan(z.I/z.R);
		 * 		
		 * 		if (z.R > 0)
		 * 			arg = atan;
		 * 		if (z.R < 0 && z.I >= 0)
		 * 			arg = atan + Math.PI;
		 * 		if (z.R < 0 && z.I < 0)
		 * 			arg = atan - Math.PI;
		 * 		if (z.R.Equals(0) && z.I > 0)
		 * 			arg = Math.PI / 2;
		 * 		if (z.R.Equals(0) && z.I < 0)
		 * 			arg = -Math.PI / 2;
		 * 		if (z == new Complex (0,0))
		 * 			arg = Double.NaN;
		 * 
		 */
		
		if (z.R.Equals(0))
			if (z.I < 0 || z.I.Equals(-1))
				arg = -Math.PI / 2;
			else if (z.I.Equals(0))
				return Double.NaN;
			else if (z.I > 0 || z.I.Equals(1))
				arg = Math.PI / 2;
		
		if (z.I.Equals(0))
			if (z.R.Equals(1))
				arg = 0;
			else if (z.R.Equals(-1))
				arg = Math.PI;
		
		if (z == new Complex(1,1))
			arg = Math.PI / 4;
		
		return arg;
	}
	
	public static double Abs(Complex z)
	{
		if (Double.IsPositiveInfinity(z.R))
			z.R = 10e99;
		else if (Double.IsNegativeInfinity(z.R))
			z.R = -10e99;
		if (Double.IsPositiveInfinity(z.I))
			z.I = 10e99;
		else if (Double.IsNegativeInfinity(z.I))
			z.I = -10e99;
		
		return Math.Sqrt(z.R*z.R + z.I*z.I);
	}
	
	public static Complex Exp(Complex z)
	{
		double r = Math.Exp(z.R);
		
		return Double.IsNaN(r) ? z : new Complex(r * Math.Cos(z.I), r * Math.Sin(z.I));
	}
	
	public static Complex Sqrt(Complex z)
	{
		double r = Abs(z);
		
		return Math.Sqrt(r) * ((z + r) / (Abs(z - r)));
	}
	
	public static Complex Sin(Complex z)
	{
		return new Complex(Math.Cos(z.R)*Math.Cosh(z.I), Math.Sin(z.R)*Math.Sinh(z.I));
	}
	
	public static Complex Cos(Complex z)
	{
		return new Complex(Math.Sin(z.R)*Math.Cosh(z.I), Math.Cos(z.R)*Math.Sinh(z.I));
	}
	
	public static Complex Tan(Complex z)
	{
		return Sin(z) / Cos(z);
	}
	
	public static Complex Sinh(Complex z)
	{
		return new Complex(Math.Cosh(z.R)*Math.Cos(z.I), Math.Sinh(z.R)*Math.Sin(z.I));
	}
	
	public static Complex Cosh(Complex z)
	{
		return new Complex(Math.Sinh(z.R)*Math.Cos(z.I), Math.Cosh(z.R)*Math.Sin(z.I));
	}
	
	public static Complex Tanh(Complex z)
	{
		return Sinh(z) / Cosh(z);
	}
	
	public static Complex Asin(Complex z)
	{
		return (new Complex(0,-1))*Ln((new Complex(0,1))*z + Sqrt(1 - (z^2)));
	}
	
	public static Complex Acos(Complex z)
	{
		return (new Complex(0,-1))*Ln(z + Sqrt((z^2) - 1));
	}
	
	public static Complex Atan(Complex z)
	{
		return (new Complex(0,1)) / 2 * (Ln(1 - (new Complex(0,1))*z) - Ln(1 + (new Complex(0,1))*z));
	}
	
	public static Complex Ln(Complex z)
	{
		return Math.Log(Abs(z)) + (new Complex(0,1)) * Arg(z);
	}
	
	public static Complex Log(Complex b, Complex z)
	{
		return Ln(z) / Ln(b);
	}
}
