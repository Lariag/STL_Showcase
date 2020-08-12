sampler2D input : register(s0);
float4 ColorX : register(C0);
float4 ColorY : register(C1);
float4 ColorZ : register(C2);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 n = tex2D(input, uv);
	float4 color;

	color.a = n.a;
	color.r = ColorX.r * n.r + ColorY.r * n.g + ColorZ.r * n.b;
	color.g = ColorX.g * n.r + ColorY.g * n.g + ColorZ.g * n.b;
	color.b = ColorX.b * n.r + ColorY.b * n.g + ColorZ.b * n.b;

	return color;
}