import {useBaseApi} from '/@/api/base';

// 入库单接口服务
export const usewmsImportLabelPrintApi = () => {
    const baseApi = useBaseApi("wmsImportLabelPrint");
    return {
        // 打印
        print: baseApi.print,
        // 重置打印流水
		reset: baseApi.reset,
        // 分页查询打印
		page: baseApi.page,
    }
}

 