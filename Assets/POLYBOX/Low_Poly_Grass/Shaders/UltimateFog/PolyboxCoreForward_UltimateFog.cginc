#ifndef POLYBOX_CORE_FORWARD_INCLUDED
#define POLYBOX_CORE_FORWARD_INCLUDED

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#	define UNITY_STANDARD_SIMPLE 1
#endif

#include "UnityStandardConfig.cginc"

#if UNITY_STANDARD_SIMPLE
	#include "UnityStandardCoreForwardSimple.cginc"
	VertexOutputBaseSimple vertBase (VertexInput v) { return vertForwardBaseSimple(v); }
	VertexOutputForwardAddSimple vertAdd (VertexInput v) { return vertForwardAddSimple(v); }
	half4 fragBase (VertexOutputBaseSimple i, fixed facing : VFACE) : SV_Target { return fragForwardBaseSimpleInternal(i, facing); }
	half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target { return fragForwardAddSimpleInternal(i); }
#else
	#include "PolyboxCore_UltimateFog.cginc"
	VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
	VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
	half4 fragBase (VertexOutputForwardBase i, fixed facing : VFACE) : SV_Target { return fragForwardBaseInternal(i, facing); }
	half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }
#endif

#endif // POLYBOX_CORE_FORWARD_INCLUDED
