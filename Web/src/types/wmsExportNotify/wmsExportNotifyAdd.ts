export interface wmsExportNotifyAddDisplayNotifyListInfter {
	allocateQuantity?: any;
	boxQuantity?: any;
	exportQuantity?: any;
	factQuantity?: any;
	importLostDate?: any;
	importProductionDate?: any;
	inspectionStatus?: any;
	inspectionStatusStr?: any;
	lotNo?: any;
	materialCode?: any;
	materialModel?: any;
	materialName?: any;
	materialStandard?: any;
	materialType?: any;
	materialTypeStr?: any;
	materialUnitId?: any;
	materialUnitStr?: any;
	materialValidityDay1?: any;
	materialValidityDay2?: any;
	materialValidityDay3?: any;
	materialValidityDayStr?: any;
	stockQuantity?: any;
	wareHouseId?: any;
	wmsBaseMaterialId?: any;
	wmsStockId?: any;
}

export interface wmsExportNotifyDetailDtoInter {
	exportBillCode: null;
	materialId?: any;
	materialCode?: any;
	materialName?: any;
	materialStandard?: any;
	materialModel?: any;
	materialType?: any;
	materialUnit?: any;
	lotNo?: any;
	productionDate?: any;
	lostDate?: any;
	warehouseId?: any;
	exportQuantity?: any;
	allocateQuantity?: any;
	factQuantity?: any;
	completeQuantity?: any;
	exportDetailFlag?: any;
	inspectionStatus?: any;
	lCLRemainderQTY?: any;
	outerDetailId?: any;
	kilogramQty?: any;

	isDelete?: any;
	createTime?: any;
	createUserName: any;
	createUserId?: any;
	updateTime?: any;
	updateUserId?: any;
	updateUserName?: any;
}
