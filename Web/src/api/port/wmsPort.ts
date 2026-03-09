import {useBaseApi} from '/@/api/base';

// 入库单接口服务
export const usewmsPortApi = () => {
    const baseApi = useBaseApi("wmsPort");
    return { 
        // 完成入库任务
        feedBack: baseApi.feedBack,
        // 入库单据下发
        createOrder: baseApi.createOrder,
    }
}

 