import { defineStore } from 'pinia';
//制作幕布（弹窗子组件开关功能）
export const useChildOpenAndClose = defineStore('useChildOpenAndClose', {
	state() {
		return {
            overLay: false, //true 幕布开启，  false 幕布关闭

            addIndex:2000,
            //"wmsExportNotify"中：wmsExportStockSlot组件
            wmsExportStockSlot_C :{	
				child_1: { //wmsExportStockSlot组件（第一个弹窗组件）
					isShow: false, //是否显示子页面
					zIndex: 100, //层级
				},
			},
            //"wmsExportNotify"中：index组件
			wmsExportNotifyIndex_C: {	
				child_1: { //wmsExportNotifyAdd组件（第一个弹窗组件）
					isShow: false, //是否显示子页面
					zIndex: 100, //层级
				},
			},
            //"wmsExportNotify"中:wmsExportNotifyAdd组件
            wmsExportNotifyAdd_C:{
                child_1:{ //selectCompany组件（第一个弹窗组件）
                    isShow: false, //是否显示子页面
					zIndex: 100, //层级
                },
                child_2:{ //wmsExportNotifyAddDetail组件（第一个弹窗组件）
                    isShow: false, //是否显示子页面
					zIndex: 100, //层级
                }
            },
            //wmsMoveOrder中：index组件
            wmsMoveOrderIndex_C:{
                child_1:{  //addMoveOrder组件（第一个弹窗组件）
                    isShow: false, //是否显示子页面
					zIndex: 100, //层级
                }
            }

		};
	},
	actions: {
        //打开模块
        open_wmsExportStockSlot_C_child_1(){
            this.wmsExportStockSlot_C.child_1.isShow = true
            this.addIndex ++;
            this.wmsExportStockSlot_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        open_wmsExportNotifyIndex_C_child_1(){
            this.wmsExportNotifyIndex_C.child_1.isShow = true
            this.addIndex ++;
            this.wmsExportNotifyIndex_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        open_wmsExportNotifyAdd_C_child_1(){
            this.wmsExportNotifyAdd_C.child_1.isShow = true
            this.addIndex ++;
            this.wmsExportNotifyAdd_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        open_wmsExportNotifyAdd_C_child_2(){
            this.wmsExportNotifyAdd_C.child_2.isShow = true
            this.addIndex ++;
            this.wmsExportNotifyAdd_C.child_2.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        open_wmsMoveOrderIndex_C_child_1(){
            this.wmsMoveOrderIndex_C.child_1.isShow = true
            this.addIndex ++;
            this.wmsMoveOrderIndex_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },




        //关闭模块
        close_wmsExportNotifyIndex_C_child_1(){
            this.wmsExportNotifyIndex_C.child_1.isShow = false
            this.addIndex --;
            this.wmsExportNotifyIndex_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        close_wmsExportNotifyAdd_C_child_1(){
            this.wmsExportNotifyAdd_C.child_1.isShow = false
            this.addIndex --;
            this.wmsExportNotifyAdd_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        close_wmsExportNotifyAdd_C_child_2(){
            this.wmsExportNotifyAdd_C.child_2.isShow = false
            this.addIndex --;
            this.wmsExportNotifyAdd_C.child_2.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        },
        close_wmsMoveOrderIndex_C_child_1(){
            this.wmsMoveOrderIndex_C.child_1.isShow = false
            this.addIndex --;
            this.wmsMoveOrderIndex_C.child_1.zIndex = this.addIndex
            this.overLay = this.addIndex >2000? true:false;
            
        }
    },
	getters: {},
});
