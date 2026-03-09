//清除reactive类型的所有数据
export function clearReactive(parm: any) {
	for (const key in parm) {
		delete parm[key];
	}
}
