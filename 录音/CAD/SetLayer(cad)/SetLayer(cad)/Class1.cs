using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SetLayer_cad_
{
    public class Class1
    {
        [CommandMethod("PassObjectToGh_CalledByGh")]
        public static void PassObjectToGh_CalledByGh()
        {

            ///定义块的名称
            string fileName = DateTime.Now.ToFileTime().ToString();

            System.Windows.Forms.Clipboard.Clear();
            System.Windows.Forms.Clipboard.SetText(fileName);

            ///在cad中选择对象
            #region
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            
           
            
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();

                ///创建块表
                BlockTable acBlkTbl= acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                
                using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                {
                    ///设置块名称
                    acBlkTblRec.Name = fileName;
                    ///设置起始点
                    acBlkTblRec.Origin = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);



                    if (acSSPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet acSSet = acSSPrompt.Value;



                        foreach (SelectedObject acSSObj in acSSet)
                        {
                            if (acSSObj != null)
                            {
                                ///读取cad对象
                                Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                                    OpenMode.ForWrite) as Entity;

                                Entity clone = acEnt.Clone() as Entity;
                                
                                acBlkTblRec.AppendEntity(clone);
                            }
                        }

                        acBlkTbl.UpgradeOpen();
                        acBlkTbl.Add(acBlkTblRec);
                        acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                    }

                }

                

                //using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                //{
                //    ///设置块名称
                //    acBlkTblRec.Name = fileName;
                //    ///设置起始点
                //    acBlkTblRec.Origin = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);


                //    if (acSSPrompt.Status == PromptStatus.OK)
                //    {
                //        SelectionSet acSSet = acSSPrompt.Value;

                //        foreach (SelectedObject acSSObj in acSSet)
                //        {
                //            if(acSSObj!=null)
                //            {
                //                ///读取cad对象
                //                Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                //                    OpenMode.ForWrite) as Entity;


                //                using (acEnt)
                //                {

                //                    acBlkTbl.UpgradeOpen();
                //                    acBlkTbl.Add(acBlkTblRec);
                //                    acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);

                //                    using (BlockReference acBlkRef = new BlockReference(new Point3d(0, 0, 0), acBlkTblRec.Id))
                //                    {
                //                        BlockTableRecord acModelSpace = 
                //                            acModelSpace = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                //                        acModelSpace.AppendEntity(acEnt);
                //                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                //                    }
                //                }

                //            }
                //        }
                //    }

                //    acTrans.Commit();
                //}
            }
            #endregion
        }
    }
}
