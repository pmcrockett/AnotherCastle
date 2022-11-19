using UnityEngine;

public class Util
{
	public static Vector3 FlattenVectorOnY(Vector3 _vector) {
		return new Vector3(_vector.x, 0, _vector.z).normalized;
	}
	public static bool IsGrounded(GameObject _obj, float _castRadius) {
		float colliderHeight = GetColliderHeight(_obj);
		RaycastHit fallCast;
		return Physics.SphereCast(_obj.transform.position, _castRadius, Vector3.down, out fallCast, 9999999, 0b10000000, QueryTriggerInteraction.Ignore)
			&& fallCast.distance <= colliderHeight / 2 + 0.01 - _castRadius;
	}

	public static float GetColliderHeight(GameObject _obj) {
		float colliderHeight;
		if (_obj.GetComponent<CapsuleCollider>()) {
			colliderHeight = _obj.GetComponent<CapsuleCollider>().height * _obj.transform.localScale.y;
			if (colliderHeight < _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x;
			if (colliderHeight < _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z;
		} else if (_obj.GetComponent<BoxCollider>()) {
			colliderHeight = _obj.GetComponent<BoxCollider>().size.y * _obj.transform.localScale.y;
		} else if (_obj.GetComponent<SphereCollider>()) {
			colliderHeight = _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.y;
			if (colliderHeight < _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.x) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.x;
			if (colliderHeight < _obj.GetComponent<SphereCollider>().radius * 2 * _obj.transform.localScale.z) colliderHeight = _obj.GetComponent<CapsuleCollider>().radius * 2 * _obj.transform.localScale.z;
		} else if (_obj.GetComponent<MeshCollider>()) {
			colliderHeight = _obj.GetComponent<MeshCollider>().bounds.size.y * _obj.transform.localScale.y;
		} else return 0;
		return colliderHeight;
    }

	public static float GetColliderWidth(GameObject _obj) {
		float colliderWidth;
		if (_obj.GetComponent<CapsuleCollider>()) {
			colliderWidth = _obj.GetComponent<CapsuleCollider>().radius * 2;
		} else if (_obj.GetComponent<BoxCollider>()) {
			colliderWidth = _obj.GetComponent<BoxCollider>().size.x < _obj.GetComponent<BoxCollider>().size.z ? _obj.GetComponent<BoxCollider>().size.x : _obj.GetComponent<BoxCollider>().size.z;
		} else if (_obj.GetComponent<SphereCollider>()) {
			colliderWidth = _obj.GetComponent<SphereCollider>().radius * 2;
		} else if (_obj.GetComponent<MeshCollider>()) {
			colliderWidth = _obj.GetComponent<MeshCollider>().bounds.size.x < _obj.GetComponent<MeshCollider>().bounds.size.z ? _obj.GetComponent<MeshCollider>().bounds.size.x : _obj.GetComponent<MeshCollider>().bounds.size.z; ;
		} else return 0;
		colliderWidth *= _obj.transform.localScale.x < _obj.transform.localScale.z ? _obj.transform.localScale.x : _obj.transform.localScale.z;
		return colliderWidth;
	}
}