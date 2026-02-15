/*
Copyright (c) Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Modified by Omar Duarte.

This file incorporates work covered by the following copyright and 
permission notice: 

Copyright (c) 2014, Nition, BSD licence. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using UnityEngine;

namespace PluginMaster
{
    public class BoundsOctreeNode
    {
        public Vector3 Center { get; private set; }
        public float BaseLength { get; private set; }
        float looseness;
        float minSize;
        float adjLength;
        Bounds bounds = default(Bounds);
        readonly System.Collections.Generic.List<OctreeObject> objects = new System.Collections.Generic.List<OctreeObject>();
        BoundsOctreeNode[] children = null;
        bool HasChildren { get { return children != null; } }
        Bounds[] childBounds;
        const int NUM_OBJECTS_ALLOWED = 8;

        struct OctreeObject
        {
            public GameObject Obj;
            public Bounds Bounds;
        }

        public BoundsOctreeNode(float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, loosenessVal, centerVal);
        }

        public bool Add(GameObject obj, Bounds objBounds)
        {
            if (!Encapsulates(bounds, objBounds))
            {
                return false;
            }
            SubAdd(obj, objBounds);
            return true;
        }

        public bool Remove(GameObject obj)
        {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Obj.Equals(obj))
                {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    removed = children[i].Remove(obj);
                    if (removed) break;
                }
            }

            if (removed && children != null)
            {
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        public bool Remove(GameObject obj, Bounds objBounds)
        {
            if (!Encapsulates(bounds, objBounds))
            {
                return false;
            }
            return SubRemove(obj, objBounds);
        }

        public bool IsColliding(ref Bounds checkBounds)
        {
            if (!bounds.Intersects(checkBounds))
            {
                return false;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Bounds.Intersects(checkBounds))
                {
                    return true;
                }
            }

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].IsColliding(ref checkBounds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsColliding(ref Ray checkRay, float maxDistance = float.PositiveInfinity)
        {
            float distance;
            if (!bounds.IntersectRay(checkRay, out distance) || distance > maxDistance)
            {
                return false;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance)
                {
                    return true;
                }
            }

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].IsColliding(ref checkRay, maxDistance))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private void FilterByInnermost(System.Collections.Generic.List<GameObject> result)
        {
            var resultArray = result.ToArray();
            result.Clear();
            for (int i = 0; i < resultArray.Length; ++i)
            {
                var go1 = resultArray[i];
                if (go1 == null) continue;
                bool go1IsInHierarchy = false;
                for (int j = 0; j < resultArray.Length; ++j)
                {
                    if (i == j) continue;
                    if (go1 == null) break;
                    var go2 = resultArray[j];
                    if (go2 == null) continue;
                    if (go1 == go2) continue;
                    if (HierarchyUtils.IsInHierarchy(go1.transform, go2.transform))
                    {
                        go1IsInHierarchy = true;
                        break;
                    }
                }
                if (!go1IsInHierarchy) result.Add(go1);
            }
        }
        public void GetColliding(ref Bounds checkBounds, System.Collections.Generic.List<GameObject> result)
        {
            if (!bounds.Intersects(checkBounds)) return;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Bounds.Intersects(checkBounds))
                    result.Add(objects[i].Obj);
            }

            FilterByInnermost(result);

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].GetColliding(ref checkBounds, result);
                }
            }
        }

        public void GetColliding(Vector3 center, Vector3 localInnerRadius,
            Quaternion gridRotation, Quaternion objectRotation, System.Collections.Generic.List<GameObject> result)
        {
            var checkSize = localInnerRadius * 1.9999f;
            var checkBounds = new Bounds(center, checkSize);
            if (!bounds.Intersects(checkBounds))
            {
                return;
            }
            var nullObjectsIndexes = new System.Collections.Generic.List<int>();
            for (int i = 0; i < objects.Count; i++)
            {
                var octreeObj = objects[i];
                if (octreeObj.Obj == null)
                {
                    nullObjectsIndexes.Insert(0, i);
                    continue;
                }
                var objCenter = octreeObj.Bounds.center;

                var fromTargetToObj = objCenter - center;
                var rotatedCellCenter = center + Quaternion.Inverse(gridRotation) * fromTargetToObj;
                var rotatedBounds = new Bounds(rotatedCellCenter, octreeObj.Bounds.size);
                if (rotatedBounds.Intersects(checkBounds))
                {
                    result.Add(objects[i].Obj);
                }
            }

            foreach (var i in nullObjectsIndexes) objects.RemoveAt(i);

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].GetColliding(center, localInnerRadius, gridRotation, objectRotation, result);
                }
            }
        }

        public void GetColliding(ref Ray checkRay, System.Collections.Generic.List<GameObject> result, float maxDistance = float.PositiveInfinity)
        {
            float distance;
            if (!bounds.IntersectRay(checkRay, out distance) || distance > maxDistance)
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Bounds.IntersectRay(checkRay, out distance) && distance <= maxDistance)
                {
                    result.Add(objects[i].Obj);
                }
            }

            FilterByInnermost(result);

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].GetColliding(ref checkRay, result, maxDistance);
                }
            }
        }

        private static bool IntersectsRay(Ray ray, Bounds bounds, float radius)
        {
            var boxCenterPlane = new Plane(-ray.direction, bounds.center);
            var sphereCenter = boxCenterPlane.ClosestPointOnPlane(ray.origin);
            var closestPoint = bounds.ClosestPoint(sphereCenter);
            var distance = (closestPoint - sphereCenter).magnitude;
            return distance <= radius;
        }

        public bool IsColliding(ref Ray checkRay, float radius, float maxDistance = float.PositiveInfinity)
        {
            if (!IntersectsRay(checkRay, bounds, radius)) return false;

            for (int i = 0; i < objects.Count; i++)
                if (IntersectsRay(checkRay, objects[i].Bounds, radius)) return true;

            if (children == null) return false;
            for (int i = 0; i < 8; i++)
                if (children[i].IsColliding(ref checkRay, radius, maxDistance)) return true;
            return false;
        }

        public void GetColliding(ref Ray checkRay, float radius, System.Collections.Generic.List<GameObject> result, float maxDistance = float.PositiveInfinity)
        {
            if (!IntersectsRay(checkRay, bounds, radius)) return;

            for (int i = 0; i < objects.Count; i++)
                if (IntersectsRay(checkRay, objects[i].Bounds, radius)) result.Add(objects[i].Obj);

            if (children == null) return;
            for (int i = 0; i < 8; i++) children[i].GetColliding(ref checkRay, radius, result, maxDistance);
        }

        public void GetWithinFrustum(Plane[] planes, System.Collections.Generic.List<GameObject> result)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (GeometryUtility.TestPlanesAABB(planes, objects[i].Bounds))
                {
                    result.Add(objects[i].Obj);
                }
            }

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    children[i].GetWithinFrustum(planes, result);
                }
            }
        }

        public void GetCollidingWithinFrustum(Plane[] planes, System.Collections.Generic.List<GameObject> result,
            Ray checkRay, float radius, Camera cam, float maxDistance = float.PositiveInfinity)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, bounds)) return;

            for (int i = 0; i < objects.Count; i++)
            {
                if (!GeometryUtility.TestPlanesAABB(planes, objects[i].Bounds)) continue;
                if (IntersectsRay(checkRay, objects[i].Bounds, radius)) result.Add(objects[i].Obj);
            }

            if (children == null) return;
            for (int i = 0; i < 8; i++)
                children[i].GetCollidingWithinFrustum(planes, result, checkRay, radius, cam, maxDistance);

        }

        public void GetCollidingWithinFrustum(Plane[] planes, System.Collections.Generic.List<(GameObject, Bounds)> result,
            Ray checkRay, float radius, Camera cam, float maxDistance = float.PositiveInfinity)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, bounds)) return;

            for (int i = 0; i < objects.Count; i++)
            {
                if (!GeometryUtility.TestPlanesAABB(planes, objects[i].Bounds)) continue;
                if (IntersectsRay(checkRay, objects[i].Bounds, radius)) result.Add((objects[i].Obj, objects[i].Bounds));
            }

            if (children == null) return;
            for (int i = 0; i < 8; i++)
                children[i].GetCollidingWithinFrustum(planes, result, checkRay, radius, cam, maxDistance);

        }

        public void SetChildren(BoundsOctreeNode[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
                return;
            }

            children = childOctrees;
        }

        public Bounds GetBounds()
        {
            return bounds;
        }

        public BoundsOctreeNode ShrinkIfPossible(float minLength)
        {
            if (BaseLength < (2 * minLength))
            {
                return this;
            }
            if (objects.Count == 0 && (children == null || children.Length == 0))
            {
                return this;
            }

            int bestFit = -1;
            for (int i = 0; i < objects.Count; i++)
            {
                OctreeObject curObj = objects[i];
                int newBestFit = BestFitChild(curObj.Bounds.center);
                if (i == 0 || newBestFit == bestFit)
                {
                    if (Encapsulates(childBounds[newBestFit], curObj.Bounds))
                    {
                        if (bestFit < 0)
                        {
                            bestFit = newBestFit;
                        }
                    }
                    else
                    {
                        return this;
                    }
                }
                else
                {
                    return this;
                }
            }

            if (children != null)
            {
                bool childHadContent = false;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].HasAnyObjects())
                    {
                        if (childHadContent)
                        {
                            return this;
                        }
                        if (bestFit >= 0 && bestFit != i)
                        {
                            return this;
                        }
                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }

            if (children == null)
            {
                SetValues(BaseLength / 2, minSize, looseness, childBounds[bestFit].center);
                return this;
            }

            if (bestFit == -1)
            {
                return this;
            }

            return children[bestFit];
        }

        public int BestFitChild(Vector3 objBoundsCenter)
        {
            return (objBoundsCenter.x <= Center.x ? 0 : 1) + (objBoundsCenter.y >= Center.y ? 0 : 4) + (objBoundsCenter.z <= Center.z ? 0 : 2);
        }

        public bool HasAnyObjects()
        {
            if (objects.Count > 0) return true;

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i].HasAnyObjects()) return true;
                }
            }

            return false;
        }

        void SetValues(float baseLengthVal, float minSizeVal, float loosenessVal, Vector3 centerVal)
        {
            BaseLength = baseLengthVal;
            minSize = minSizeVal;
            looseness = loosenessVal;
            Center = centerVal;
            adjLength = looseness * baseLengthVal;

            Vector3 size = new Vector3(adjLength, adjLength, adjLength);
            bounds = new Bounds(Center, size);

            float quarter = BaseLength / 4f;
            float childActualLength = (BaseLength / 2) * looseness;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            childBounds = new Bounds[8];
            childBounds[0] = new Bounds(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            childBounds[1] = new Bounds(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            childBounds[2] = new Bounds(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            childBounds[3] = new Bounds(Center + new Vector3(quarter, quarter, quarter), childActualSize);
            childBounds[4] = new Bounds(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            childBounds[5] = new Bounds(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            childBounds[6] = new Bounds(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            childBounds[7] = new Bounds(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        void SubAdd(GameObject obj, Bounds objBounds)
        {
            if (!HasChildren)
            {
                if (objects.Count < NUM_OBJECTS_ALLOWED || (BaseLength / 2) < minSize)
                {
                    OctreeObject newObj = new OctreeObject { Obj = obj, Bounds = objBounds };
                    objects.Add(newObj);
                    return;
                }

                int bestFitChild;
                if (children == null)
                {
                    Split();
                    if (children == null)
                    {
                        Debug.LogError("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }

                    for (int i = objects.Count - 1; i >= 0; i--)
                    {
                        OctreeObject existingObj = objects[i];
                        bestFitChild = BestFitChild(existingObj.Bounds.center);
                        if (Encapsulates(children[bestFitChild].bounds, existingObj.Bounds))
                        {
                            children[bestFitChild].SubAdd(existingObj.Obj, existingObj.Bounds);
                            objects.Remove(existingObj);
                        }
                    }
                }
            }

            int bestFit = BestFitChild(objBounds.center);
            if (Encapsulates(children[bestFit].bounds, objBounds))
            {
                children[bestFit].SubAdd(obj, objBounds);
            }
            else
            {
                OctreeObject newObj = new OctreeObject { Obj = obj, Bounds = objBounds };
                objects.Add(newObj);
            }
        }

        bool SubRemove(GameObject obj, Bounds objBounds)
        {
            bool removed = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Obj.Equals(obj))
                {
                    removed = objects.Remove(objects[i]);
                    break;
                }
            }

            if (!removed && children != null)
            {
                int bestFitChild = BestFitChild(objBounds.center);
                removed = children[bestFitChild].SubRemove(obj, objBounds);
            }

            if (removed && children != null)
            {
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        void Split()
        {
            float quarter = BaseLength / 4f;
            float newLength = BaseLength / 2;
            children = new BoundsOctreeNode[8];
            children[0] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(-quarter, quarter, -quarter));
            children[1] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(quarter, quarter, -quarter));
            children[2] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(-quarter, quarter, quarter));
            children[3] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(quarter, quarter, quarter));
            children[4] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(-quarter, -quarter, -quarter));
            children[5] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(quarter, -quarter, -quarter));
            children[6] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(-quarter, -quarter, quarter));
            children[7] = new BoundsOctreeNode(newLength, minSize, looseness, Center + new Vector3(quarter, -quarter, quarter));
        }

        void Merge()
        {
            for (int i = 0; i < 8; i++)
            {
                BoundsOctreeNode curChild = children[i];
                int numObjects = curChild.objects.Count;
                for (int j = numObjects - 1; j >= 0; j--)
                {
                    OctreeObject curObj = curChild.objects[j];
                    objects.Add(curObj);
                }
            }
            children = null;
        }

        static bool Encapsulates(Bounds outerBounds, Bounds innerBounds)
        {
            return outerBounds.Contains(innerBounds.min) && outerBounds.Contains(innerBounds.max);
        }

        bool ShouldMerge()
        {
            int totalObjects = objects.Count;
            if (children != null)
            {
                foreach (BoundsOctreeNode child in children)
                {
                    if (child.children != null)
                    {
                        return false;
                    }
                    totalObjects += child.objects.Count;
                }
            }
            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }

        public void GetColliding(ref Ray checkRay, System.Collections.Generic.List<GameObject> result,
            float radius, float maxDistance = float.PositiveInfinity)
        {
            float nodeDistance;
            if (!bounds.IntersectRay(checkRay, out nodeDistance) || nodeDistance > maxDistance)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                var objBounds = objects[i].Bounds;
                if (RayIntersectsBoxPlanes(checkRay, objBounds, radius, maxDistance))
                {
                    result.Add(objects[i].Obj);
                }
            }

            if (children != null)
            {
                for (int i = 0; i < 8; i++)
                    children[i].GetColliding(ref checkRay, result, radius, maxDistance);
            }
        }

        private static bool RayIntersectsBoxPlanes(Ray ray, Bounds bounds, float radius, float maxDistance)
        {
            Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            for (int axis = 0; axis < 3; axis++)
            {
                for (int side = 0; side < 2; side++)
                {
                    Vector3 normal = axes[axis] * (side == 0 ? -1f : 1f);
                    float d = side == 0 ? min[axis] : max[axis];

                    Plane plane = new Plane(normal, d);

                    float denom = Vector3.Dot(ray.direction, normal);
                    if (Mathf.Abs(denom) < 1e-6f) continue;

                    float t = (d - Vector3.Dot(ray.origin, normal)) / denom;
                    if (t < 0 || t > maxDistance) continue;

                    Vector3 pointOnRay = ray.origin + ray.direction * t;

                    Vector3 clamped = pointOnRay;
                    for (int j = 0; j < 3; j++)
                    {
                        if (j == axis) clamped[j] = d;
                        else clamped[j] = Mathf.Clamp(clamped[j], min[j], max[j]);
                    }

                    float dist = Vector3.Distance(pointOnRay, clamped);
                    if (dist < radius)
                        return true;
                }
            }
            return false;
        }
    }

    public class BoundsOctree
    {
        public int Count { get; private set; }
        BoundsOctreeNode rootNode;
        readonly float looseness;
        readonly float initialSize;
        readonly float minSize;

        public BoundsOctree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize, float loosenessVal)
        {
            if (minNodeSize > initialWorldSize)
            {
                Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            initialSize = initialWorldSize;
            minSize = minNodeSize;
            looseness = Mathf.Clamp(loosenessVal, 1.0f, 2.0f);
            rootNode = new BoundsOctreeNode(initialSize, minSize, looseness, initialWorldPos);
        }

        public void Add(GameObject obj, Bounds objBounds)
        {
            int count = 0;
            while (!rootNode.Add(obj, objBounds))
            {
                Grow(objBounds.center - rootNode.Center);
                if (++count > 20)
                {
                    Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (count - 1) + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        public bool Remove(GameObject obj)
        {
            bool removed = rootNode.Remove(obj);

            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        public bool Remove(GameObject obj, Bounds objBounds)
        {
            bool removed = rootNode.Remove(obj, objBounds);

            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        public bool IsColliding(Bounds checkBounds)
        {
            return rootNode.IsColliding(ref checkBounds);
        }

        public bool IsColliding(Ray checkRay, float maxDistance)
        {
            return rootNode.IsColliding(ref checkRay, maxDistance);
        }

        public void GetColliding(System.Collections.Generic.List<GameObject> collidingWith, Bounds checkBounds)
        {
            rootNode.GetColliding(ref checkBounds, collidingWith);
        }

        public void GetColliding(Vector3 center, Vector3 localInnerRadius, Quaternion gridRotation, Quaternion objectRotation,
            System.Collections.Generic.List<GameObject> result)
        {
            rootNode.GetColliding(center, localInnerRadius, gridRotation, objectRotation, result);
        }

        public void GetColliding(System.Collections.Generic.List<GameObject> collidingWith, Ray checkRay, float maxDistance = float.PositiveInfinity)
        {
            rootNode.GetColliding(ref checkRay, collidingWith, maxDistance);
        }

        public GameObject[] GetColliding(Ray checkRay, float maxDistance = float.PositiveInfinity)
        {
            var collidingWith = new System.Collections.Generic.List<GameObject>();
            rootNode.GetColliding(ref checkRay, collidingWith, maxDistance);
            return collidingWith.ToArray();
        }

        public System.Collections.Generic.List<GameObject> GetWithinFrustum(Camera cam)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            var list = new System.Collections.Generic.List<GameObject>();
            rootNode.GetWithinFrustum(planes, list);
            return list;
        }

        public bool GetCollidingtWithinFrustum(Ray checkRay, float radius,
            Camera cam, out (GameObject, Bounds)[] collidingWith, float maxDistance = float.PositiveInfinity)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            var result = new System.Collections.Generic.List<(GameObject, Bounds)>();
            rootNode.GetCollidingWithinFrustum(planes, result, checkRay, radius, cam, maxDistance);
            collidingWith = result.ToArray();
            return collidingWith.Length > 0;
        }

        public Bounds GetMaxBounds()
        {
            return rootNode.GetBounds();
        }

        void Grow(Vector3 direction)
        {
            int xDirection = direction.x >= 0 ? 1 : -1;
            int yDirection = direction.y >= 0 ? 1 : -1;
            int zDirection = direction.z >= 0 ? 1 : -1;
            BoundsOctreeNode oldRoot = rootNode;
            float half = rootNode.BaseLength / 2;
            float newLength = rootNode.BaseLength * 2;
            Vector3 newCenter = rootNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            rootNode = new BoundsOctreeNode(newLength, minSize, looseness, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                int rootPos = rootNode.BestFitChild(oldRoot.Center);
                BoundsOctreeNode[] children = new BoundsOctreeNode[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new BoundsOctreeNode(oldRoot.BaseLength, minSize, looseness, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                rootNode.SetChildren(children);
            }
        }

        void Shrink()
        {
            rootNode = rootNode.ShrinkIfPossible(initialSize);
        }

        public void GetColliding(System.Collections.Generic.List<GameObject> collidingWith, Ray checkRay, float radius,
            float maxDistance)
        {
            rootNode.GetColliding(ref checkRay, collidingWith, radius, maxDistance);
        }
    }
}