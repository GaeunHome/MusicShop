// ===== 超商門市模擬資料 =====
// 此為模擬資料，實際應用需整合超商 API

const StoreData = {
    // 7-11 門市資料
    sevenEleven: [
        { code: '171899', name: '台北車站門市', address: '台北市中正區忠孝西路一段49號', city: '台北市', district: '中正區' },
        { code: '171902', name: '西門町門市', address: '台北市萬華區漢中街32號', city: '台北市', district: '萬華區' },
        { code: '171915', name: '信義新天地門市', address: '台北市信義區松壽路11號', city: '台北市', district: '信義區' },
        { code: '171928', name: '師大夜市門市', address: '台北市大安區師大路39巷3號', city: '台北市', district: '大安區' },
        { code: '171931', name: '東區商圈門市', address: '台北市大安區忠孝東路四段181巷7弄9號', city: '台北市', district: '大安區' },
        { code: '172801', name: '板橋車站門市', address: '新北市板橋區縣民大道二段7號', city: '新北市', district: '板橋區' },
        { code: '172814', name: '淡水老街門市', address: '新北市淡水區中正路156號', city: '新北市', district: '淡水區' },
        { code: '172827', name: '三重重新門市', address: '新北市三重區重新路三段58號', city: '新北市', district: '三重區' },
        { code: '173701', name: '桃園車站門市', address: '桃園市桃園區中正路20號', city: '桃園市', district: '桃園區' },
        { code: '173714', name: '中壢中央門市', address: '桃園市中壢區中央西路二段55號', city: '桃園市', district: '中壢區' },
        { code: '174601', name: '台中車站門市', address: '台中市中區建國路172號', city: '台中市', district: '中區' },
        { code: '174614', name: '逢甲商圈門市', address: '台中市西屯區文華路100號', city: '台中市', district: '西屯區' },
        { code: '174627', name: '一中街門市', address: '台中市北區一中街68號', city: '台中市', district: '北區' },
        { code: '175501', name: '台南車站門市', address: '台南市中西區中山路125號', city: '台南市', district: '中西區' },
        { code: '175514', name: '成功大學門市', address: '台南市東區大學路1號', city: '台南市', district: '東區' },
        { code: '176401', name: '高雄車站門市', address: '高雄市三民區建國二路320號', city: '高雄市', district: '三民區' },
        { code: '176414', name: '美麗島站門市', address: '高雄市新興區中山一路115號', city: '高雄市', district: '新興區' },
        { code: '176427', name: '夢時代門市', address: '高雄市前鎮區中華五路789號', city: '高雄市', district: '前鎮區' }
    ],

    // 全家門市資料
    familyMart: [
        { code: '001256', name: '台北車站店', address: '台北市中正區北平西路3號', city: '台北市', district: '中正區' },
        { code: '001269', name: '西門町店', address: '台北市萬華區武昌街二段50號', city: '台北市', district: '萬華區' },
        { code: '001272', name: '信義威秀店', address: '台北市信義區松壽路20號', city: '台北市', district: '信義區' },
        { code: '001285', name: '師大店', address: '台北市大安區師大路93號', city: '台北市', district: '大安區' },
        { code: '001298', name: '忠孝復興店', address: '台北市大安區忠孝東路三段248號', city: '台北市', district: '大安區' },
        { code: '002167', name: '板橋車站店', address: '新北市板橋區站前路5號', city: '新北市', district: '板橋區' },
        { code: '002170', name: '淡水捷運店', address: '新北市淡水區中正東路一段1號', city: '新北市', district: '淡水區' },
        { code: '002183', name: '三重重陽店', address: '新北市三重區重陽路一段88號', city: '新北市', district: '三重區' },
        { code: '003056', name: '桃園車站店', address: '桃園市桃園區復興路126號', city: '桃園市', district: '桃園區' },
        { code: '003069', name: '中壢中正店', address: '桃園市中壢區中正路268號', city: '桃園市', district: '中壢區' },
        { code: '004945', name: '台中車站店', address: '台中市中區建國路145號', city: '台中市', district: '中區' },
        { code: '004958', name: '逢甲店', address: '台中市西屯區福星路428號', city: '台中市', district: '西屯區' },
        { code: '004961', name: '一中店', address: '台中市北區一中街91號', city: '台中市', district: '北區' },
        { code: '005834', name: '台南車站店', address: '台南市中西區北門路一段90號', city: '台南市', district: '中西區' },
        { code: '005847', name: '成大店', address: '台南市東區大學路西段53號', city: '台南市', district: '東區' },
        { code: '006723', name: '高雄車站店', address: '高雄市三民區建國二路295號', city: '高雄市', district: '三民區' },
        { code: '006736', name: '美麗島店', address: '高雄市新興區中山一路162號', city: '高雄市', district: '新興區' },
        { code: '006749', name: '夢時代店', address: '高雄市前鎮區中華五路789號B1', city: '高雄市', district: '前鎮區' }
    ],

    /**
     * 根據超商類型取得門市清單
     * @param {string} storeType - 'sevenEleven' 或 'familyMart'
     * @param {string} searchKeyword - 搜尋關鍵字（選填）
     * @returns {Array} 門市清單
     */
    getStores(storeType, searchKeyword = '') {
        const stores = this[storeType] || [];

        if (!searchKeyword.trim()) {
            return stores;
        }

        // 模糊搜尋：門市名稱、地址、縣市、鄉鎮
        const keyword = searchKeyword.toLowerCase();
        return stores.filter(store =>
            store.name.toLowerCase().includes(keyword) ||
            store.address.toLowerCase().includes(keyword) ||
            store.city.toLowerCase().includes(keyword) ||
            store.district.toLowerCase().includes(keyword) ||
            store.code.includes(keyword)
        );
    },

    /**
     * 根據縣市篩選門市
     * @param {string} storeType - 'sevenEleven' 或 'familyMart'
     * @param {string} city - 縣市名稱
     * @returns {Array} 門市清單
     */
    getStoresByCity(storeType, city) {
        const stores = this[storeType] || [];

        if (!city) {
            return stores;
        }

        return stores.filter(store => store.city === city);
    },

    /**
     * 取得所有縣市清單
     * @param {string} storeType - 'sevenEleven' 或 'familyMart'
     * @returns {Array} 縣市清單（不重複）
     */
    getCities(storeType) {
        const stores = this[storeType] || [];
        const cities = [...new Set(stores.map(store => store.city))];
        return cities.sort();
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = StoreData;
}
