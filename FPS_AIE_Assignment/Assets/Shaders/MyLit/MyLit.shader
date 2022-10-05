

Shader "CustomTestShaders/MyLit"{
	// Properties are options set per material, exposed by the material inspector
	Properties{
		[Header(Surface options)] // Creates a text header
		// [MainTexture] and [MainColor] allows Material.mainTexture & Material.color to use the correct property
		[MainTexture] _ColorMap("Color", 2D) = "white" {}
		[MainColor] _ColorTint("Tint", Color) = (1, 1, 1, 1)
		_Smoothness("Smoothness", Float) = 0
	}

	// Subshaders allow for different behaviour and options for different pipelines and platforms
	SubShader{
		// These tags are shared by all passes in this sub shader
		Tags { "RenderPipeline" = "UniversalPipeline" } // Sets render pipeline
		//this tells unity to use this shader when the UniversalPipeline is being used

		// Shaders can have several passes which are used to render different data about the material
		// Each pass has it's own vertex and fragment function and shader variant keywords
		Pass {
			Name "ForwardLit"						  // For debugging
			Tags { "LightMode" = "UniversalForward" } // Pass specific tags
			// "UniversalForward" tells Unity this is the main lighting pass of this shader

			HLSLPROGRAM // Begin HLSL code

			#define _SPECULAR_COLOR
#if UNITY_VERSION >= 202120
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#else
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#endif
			#pragma multi_compile_fragment _ _SHADOWS_SOFT

			// Register our programmable stage functions
			#pragma vertex Vertex
			#pragma fragment Fragment

			// Include our code file
			#include "MyLitForwardLitPass.hlsl"

			ENDHLSL
		}

		Pass {
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}

			ColorMask 0

			HLSLPROGRAM

			#pragma vertex Vertex
			#pragma fragment Fragment

			#include "MyLitShadowCasterPass.hlsl"
			ENDHLSL
		}
	}
}




//// Pull in URP library functions and our own common functions
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//
//// This attributes struct receives data about the mesh we're currently rendering
//// Data is automatically placed in fields according to their semantic
//struct Attributes {
//	float3 position : POSITION; // Position in object space
//};
//void Vertex(Attributes input) {
//	// These helper functions, found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
//	// transform object space values into world and clip space
//	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.position);
//
//	// Pass position and orientation data to the fragment function
//	float4 positionCS = posnInputs.positionCS;
//}
//
//
//
//struct Interpolators {
//	// This value should contain the position in clip space (which is similar to a position on screen)
//	// when output from the vertex function. It will be transformed into pixel position of the current
//	// fragment on the screen when read from the fragment function
//	float4 positionCS : SV_POSITION;
//};
//
//// The vertex function. this runs for each vertex on the mesh.
//// It must output the position on the screen each vertex should appear at,
//// as well as any data the fragment function will need
//Interpolators Vertex(Attributes input) {
//	Interpolators output;
//
//	// These helper functions, found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
//	// transform object space values into world and clip space
//	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
//
//	// Pass position and orientation data to the fragment function
//	output.positionCS = posnInputs.positionCS;
//
//	return output;
//}

