export function getDate(n:number){
    let date = new Date();
    let targetDate = new Date(date);
    // 减去三天（3天的毫秒数）
    targetDate.setDate(targetDate.getDate() + n);

    const year = targetDate.getFullYear()
    const month = String(targetDate.getMonth() + 1).padStart(2, '0')
    const day = String(targetDate.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
}