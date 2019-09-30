using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dynagon {

	public class Polygon3D : Polygon {

		public Polygon3D(GameObject gameObject, List<Vector3> vertices) : base(gameObject, vertices) {}
		public Polygon3D(GameObject gameObject, List<Vector3> vertices, Color col, bool hasCollider=true) : base(gameObject, vertices, col) {
            if (hasCollider)
            {
                var collider = gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
            }
        }

		protected Vector3 GetCentroid() {
			var uniqs = vertices.Distinct();
			return uniqs.Aggregate(Vector3.zero, (sum, v) => (sum + v)) / uniqs.Count();
		}

		protected override void OptimizeIndexes() {
			var centroid = GetCentroid();
			foreach (var i in Enumerable.Range(0, indexes.Count/3).Select(i => i * 3)) {
				var center = GetCenterOfTriangle(i);
				if (Vector3.Dot(GetNormalOfTriangle(i), center - centroid) < 0) {
					ReverseSurface(i);
				}
			}
		}
		
	}

}
