﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CraftpiaViewSaveData.CommonConst;
using static CraftpiaViewSaveData.NestParams.ConvertCraftpiaParams;
using CraftpiaViewSaveData.NestParams;
using CraftpiaViewSaveData.File;

namespace CraftpiaViewSaveData
{
    public partial class MainForm : Form
    {
        List<ClassDb> originalData;
        CraftpiaParams convertData;
        Dictionary<string, CraftpiaParams> eachData;

        public MainForm()
        {
            InitializeComponent();
        }

        #region "データ取得関係"
        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            var ocss = files.Where(f => Path.GetExtension(f) == ".db" || Path.GetExtension(f) == ".json");
            if (ocss.Count() != 1) return;

            //originalData = ImportFile.Import(ocss.First());
            //convertData = ImportFile.GetList(originalData, ocss.First());
            originalData = CrudDb.Read(ocss.First());
            convertData = ConvertCraftpiaParams.JsonStrToCraftpiaParams(originalData.Where(p => p.id == PPSave_ID_InGame).First().value, ocss.First());
            CraftpiaParamsToCPTree(convertData);

            eachData = convertData.GetChildParamsString();

            //dgvにセットしていく...
            //以下の構造である
            //value -- {charaMakeData,plStatusSaveData,inventorySaveData}
            //inventorySaveData -- {equipmentList,buildingList,consumptionList,personalChestList,petList,materialList,petChestList}
            //                      └-- itemInBox -- {(item,count) * 最大4枠}
            dgv1.Rows.Clear();

            var viewlist = GetResourceFile.GetFile();
            int row = 0;
            foreach (var data in eachData)
            {
                var dataTree = data.Key.Split('-').ToList();

                dgv1.Rows.Add();
                dgv1.Rows[row].Cells[(int)rowindex.CategoryName].Value = dataTree[2];
                dgv1.Rows[row].Cells[(int)rowindex.DetailName].Value = data.Value.name;
                dgv1.Rows[row].Cells[(int)rowindex.FullName].Value = data.Key;

                //名称をtxtから取得
                foreach (var v in viewlist)
                {
                    int idx = -1;
                    bool ismatch = true;
                    foreach (var k in v.Key.Split('.'))
                    {
                        if (idx >= dataTree.IndexOf(k))
                        {
                            ismatch = false;
                            break;
                        }
                    }
                    if (ismatch)
                    {
                        dgv1.Rows[row].Cells[(int)rowindex.DetailName_Ja].Value = viewlist[v.Key];
                    }
                }
                //何個目のアイテムか,Noを発番
                var no_str = "";
                foreach (var d in dataTree)
                {
                    if (int.TryParse(d, out int i))
                        no_str += "-" + i.ToString();
                }
                dgv1.Rows[row].Cells[(int)rowindex.Numbering].Value = no_str == "" ? "1" : no_str.TrimStart('-');

                dgv1.Rows[row].Cells[(int)rowindex.Value].Value = data.Value.value; //値
                dgv1.Rows[row].Cells[(int)rowindex.Index].Value = data.Value.index; //index
                dgv1.Rows[row].Cells[(int)rowindex.HexIndex].Value = Convert.ToString(data.Value.index, 16); //index0x
                row++;
            }
        }
        #endregion
        #region "アイテム詳細関係"

        private void panelEquipment_content_Click(object sender, EventArgs e)
        {
            //c#にはLike文が無いので面倒
            var PanelArray1 = panelEquipment.Controls.OfType<Panel>()
                                  .Where(p => System.Text.RegularExpressions.Regex.IsMatch(p.Name, "^p1_.*", RegexOptions.Singleline))
                                  .OrderBy(q => q.TabIndex).ToArray();

            var name = ((Panel)sender).Name;
            var categoryIndex = 1;
            var itemIndex = Convert.ToInt32(name.Split('_').Last());

            DisplayItemDetail(categoryIndex, itemIndex);
        }

        void DisplayItemDetail(int categoryIndex, int itemindex)
        {

        }
        #endregion
    }
}
