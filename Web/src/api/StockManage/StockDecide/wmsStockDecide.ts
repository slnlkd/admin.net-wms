import { useBaseApi } from '/@/api/base';

// 质检改判接口服务
export const useWmsStockDecideApi = () => {
  const baseApi = useBaseApi('wmsStockDecide');
  return {
    // 分页查询
    page: baseApi.page,
    // 明细反填
    detailList: (data: any, cancel: boolean = false) => baseApi.request({ url: baseApi.baseUrl + 'detailList', method: 'post', data }, cancel),
    // 改判前校验
    check: (data: any, cancel: boolean = false) => baseApi.request({ url: baseApi.baseUrl + 'check', method: 'post', data }, cancel),
    // 改判
    decide: (data: any, cancel: boolean = false) => baseApi.request({ url: baseApi.baseUrl + 'decide', method: 'post', data }, cancel),
    // 放行/取消放行
    release: (data: any, cancel: boolean = false) => baseApi.request({ url: baseApi.baseUrl + 'release', method: 'post', data }, cancel),
  };
};
