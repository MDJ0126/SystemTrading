using System.ComponentModel;

/// <summary>
/// 보유 주식 상태
/// </summary>
public enum eBalanceStockState
{
	None,
	/// <summary>
	/// 매수 신청
	/// </summary>
	RequestBuy,
	/// <summary>
	/// 매수 진행 중
	/// </summary>
	Buying,
	/// <summary>
	/// 매도 신청
	/// </summary>
	RequestSell,
	/// <summary>
	/// 매도 진행 중
	/// </summary>
	Selling,
	/// <summary>
	/// 보유 중
	/// </summary>
	Have,
}

public enum eMarketIndexType
{
	none,
	KOSPI,
	KOSPI200,
	KOSDAQ,
}

public enum eChartType
{
	틱 = 0,
	분봉,
	일봉,
	주봉,
	월봉,
}

public enum eOrderType
{
	매도10호가 = 0,
	매도9호가,
	매도8호가,
	매도7호가,
	매도6호가,
	매도5호가,
	매도4호가,
	매도3호가,
	매도2호가,
	매도최우선호가,
	매수최우선호가,
	매수2호가,
	매수3호가,
	매수4호가,
	매수5호가,
	매수6호가,
	매수7호가,
	매수8호가,
	매수9호가,
	매수10호가,
}

public enum eFID : int
{
	현재가 = 10,
	전일대비 = 11,
	등락율 = 12,
	누적거래량 = 13,
	누적거래대금 = 14,
	거래량 = 15,
	시가 = 16,
	고가 = 17,
	저가 = 18,
	체결시간 = 20,
	전일대비기호 = 25,
	전일거래량대비_계약_주 = 26,
	최우선_매도호가 = 27,
	최우선_매수호가 = 28,
	거래대금증감 = 29,
	전일거래량대비_비율 = 30,
	거래회전율 = 31,
	거래비용 = 32,
	장시작예상잔여시간 = 214,
	장운영구분 = 215,
	종목명 = 302,
	시가총액_억 = 311,
	상한가발생시간 = 567,
	하한가발생시간 = 568,
	주문수량 = 900,
	주문가격 = 901,
	미체결수량 = 902,
	체결누계금액 = 903,
	주문구분 = 905,
	주문체결시간 = 908,
	체결가 = 910,
	체결량_누적 = 911, // 누적 체결량 같음
	주문상태 = 913,
	단위체결가 = 914,
	화면번호 = 920,
	총매입가 = 932,
	당일매매수수료 = 938,
	당일매매세금 = 939,
	당일총매도손일 = 950,
	손익율 = 8019,
	종목코드_업종코드 = 9001,
	계좌번호 = 9201,
	주문번호 = 9203,
}

/// <summary>
/// 장 운영 구분
/// </summary>
public enum eTradingTimeState
{
	장_미운영시간 = -1,
	장시작전 = 0,
	장_종료_10분전_동시호가 = 2,
	장_중 = 3,
	장_종료 = 4,
}

/// <summary>
/// OnReceiveChejanData() Gubun enum
/// </summary>
public enum eSendOrderResultType
{ 
	체결 = 0,
	국내주식잔고변경 = 1,
	파생잔고변경 = 4,
}


/// <summary>
/// SendOrder 주문 타입
/// </summary>
public enum eSendOrderType
{
	None     = 0,
	신규매수 = 1,
	신규매도 = 2,
	매수취소 = 3,
	매도취소 = 4,
	매수정정 = 5,
	매도정정 = 6,
}

/// <summary>
/// 거래 구분
/// </summary>
public enum eSendType
{
	[Description("00")]
	지정가 = 0,
	[Description("03")]
	시장가,
	[Description("05")]
	조건부지정가,
	[Description("06")]
	최유리지정가,
	[Description("07")]
	최우선지정가,
	[Description("10")]
	지정가IOC,
	[Description("13")]
	시장가IOC,
	[Description("16")]
	최유리IOC,
	[Description("20")]
	지정가FOK,
	[Description("23")]
	시장가FOK,
	[Description("26")]
	최유리FOK,
	[Description("61")]
	장전시간외종가,
	[Description("62")]
	시간외단일가매매,
	[Description("81")]
	장후시간외종가,
}

/// <summary>
/// Transaction 요청
/// </summary>
public enum eOPTCode
{
	[Description("Unknown")]
	Unknown,
	[Description("Unknown")]
	신규매수,
	[Description("Unknown")]
	신규매도,
	[Description("Unknown")]
	매수취소,
	[Description("Unknown")]
	매도취소,
	[Description("Unknown")]
	매수정정,
	[Description("Unknown")]
	매도정정,
	[Description("OPT10001")]
	주식기본정보요청,
	[Description("OPT10002")]
	주식거래원요청,
	[Description("OPT10003")]
	체결정보요청,
	[Description("OPT10004")]
	주식호가요청,
	[Description("OPT10005")]
	주식일주월시분요청,
	[Description("OPT10006")]
	주식시분요청,
	[Description("OPT10007")]
	시세표성정보요청,
	[Description("OPT10008")]
	주식외국인요청,
	[Description("OPT10009")]
	주식기관요청,
	[Description("OPT10010")]
	업종프로그램요청,
	[Description("OPT10011")]
	신주인수권전체시세요청,
	[Description("OPT10012")]
	주문체결요청,
	[Description("OPT10013")]
	신용매매동향요청,
	[Description("OPT10014")]
	공매도추이요청,
	[Description("OPT10015")]
	일별거래상세요청,
	[Description("OPT10016")]
	신고저가요청,
	[Description("OPT10017")]
	상하한가요청,
	[Description("OPT10018")]
	고가가근접요청,
	[Description("OPT10019")]
	가격급락요청,
	[Description("OPT10020")]
	호가잔량상위요청,
	[Description("OPT10021")]
	호가잔량급증요청,
	[Description("OPT10022")]
	잔량율급증요청,
	[Description("OPT10023")]
	거래량급증요청,
	[Description("OPT10024")]
	거래량갱신요청,
	[Description("OPT10025")]
	매물대집중요청,
	[Description("OPT10026")]
	고저PER요청,
	[Description("OPT10027")]
	전일대비등락률상위요청,
	[Description("OPT10028")]
	시가대비등락률상위요청,
	[Description("OPT10029")]
	예상체결등락률상위요청,
	[Description("OPT10030")]
	당일거래량상위요청,
	[Description("OPT10031")]
	전일거래량상위요청,
	[Description("OPT10032")]
	거래대금상위요청,
	[Description("OPT10033")]
	신용비율상위요청,
	[Description("OPT10034")]
	외인기간별매매상위요청,
	[Description("OPT10035")]
	외인연속순매매상위요청,
	[Description("OPT10036")]
	외인한동소진율증가상위,
	[Description("OPT10037")]
	외국계창구매매상위요청,
	[Description("OPT10038")]
	종목별증권사순위요청,
	[Description("OPT10039")]
	증권사별매매상위요청,
	[Description("OPT10040")]
	당일주요거래원요청,
	[Description("OPT10041")]
	조기종료통화단위요청,
	[Description("OPT10042")]
	순매수거래원순위요청,
	[Description("OPT10043")]
	거래원매물대분석요청,
	[Description("OPT10044")]
	일별기관매매종목요청,
	[Description("OPT10045")]
	종목별기관매매추이요청,
	[Description("OPT10046")]
	체결강도추이시간별요청,
	[Description("OPT10047")]
	체결강도추이일별요청,
	[Description("OPT10048")]
	ELW일별민감도지표요청,
	[Description("OPT10049")]
	ELW투자지표요청,
	[Description("OPT10050")]
	ELW민감도지표요청,
	[Description("OPT10051")]
	업종별투자자순매수요청,
	[Description("OPT10052")]
	거래원순간거래량요청,
	[Description("OPT10053")]
	당일상위이탈원요청,
	[Description("OPT10054")]
	변동성완화장치발동종목요청,
	[Description("OPT10055")]
	당일전일체결대량요청,
	[Description("OPT10058")]
	투자자별일별매매종목요청,
	[Description("OPT10059")]
	종목별투자자기관별요청,
	[Description("OPT10060")]
	종목별투자자기관별차트요청,
	[Description("OPT10061")]
	종목별투자자기관별합계요청,
	[Description("OPT10062")]
	동일순매매순위요청,
	[Description("OPT10063")]
	장중투자자별매매요청,
	[Description("OPT10064")]
	장중투자자별매매차트요청,
	[Description("OPT10065")]
	장중투자자별매매상위요청,
	//[Description("OPT10066")]
	//장중투자자별매매차트요청,	// == OPT10064
	[Description("OPT10067")]
	대차거래내역요청,
	[Description("OPT10068")]
	대차거래추이요청,
	[Description("OPT10069")]
	대차거래상위10종목요청,
	//[Description("OPT10070")]
	//당일주요거래원요청,	// == OPT10070
	[Description("OPT10071")]
	시간대별전일비거래비중요청,
	//[Description("OPT10072")]
	//일자별종목별실현손익요청,	// == OPT10073
	[Description("OPT10073")]
	일자별종목별실현손익요청,
	[Description("OPT10074")]
	일자별실현손익요청,
	[Description("OPT10075")]
	미체결요청,
	[Description("OPT10076")]
	체결요청,
	[Description("OPT10077")]
	당일실현손익상세요청,
	[Description("OPT10078")]
	증권사별종목매매동향요청,
	[Description("OPT10079")]
	주식틱차트조회요청,
	[Description("OPT10080")]
	주식분봉차트조회요청,
	[Description("OPT10081")]
	주식일봉차트조회요청,
	[Description("OPT10082")]
	주식주봉차트조회요청,
	[Description("OPT10083")]
	주식월봉차트조회요청,
	[Description("OPT10084")]
	당일전일체결요청,
	[Description("OPT10085")]
	계좌수익률요청,
	[Description("OPT10086")]
	일별주가요청,
	[Description("OPT10087")]
	시간외단일가요청,
	[Description("OPT10094")]
	주식년봉차트조회요청,
	[Description("OPT20001")]
	업종현재가요청,
	[Description("OPT20002")]
	업종별주가요청,
	[Description("OPT20003")]
	전업종지수요청,
	[Description("OPT20004")]
	업종틱차트조회요청,
	[Description("OPT20005")]
	업종분봉조회요청,
	[Description("OPT20006")]
	업종일봉조회요청,
	[Description("OPT20007")]
	업종주봉조회요청,
	[Description("OPT20008")]
	업종월봉조회요청,
	[Description("OPT20009")]
	업종현재가일별요청,
	[Description("OPT20019")]
	업종년봉조회요청,
	[Description("OPT20068")]
	대차거래추이요청_종목별,
	[Description("OPT30001")]
	ELW가격급등락요청,
	[Description("OPT30002")]
	거래원별ELW순매매상위요청,
	[Description("OPT30003")]
	ELWLP보유일별추이요청,
	[Description("OPT30004")]
	ELW괴리율요청,
	[Description("OPT30005")]
	ELW조건검색요청,
	//[Description("OPT30006")]
	//ELW종목상세요청,	// 미지원
	//[Description("OPT30007")]
	//ELW종목상세요청,	// 미지원
	//[Description("OPT30008")]
	//ELW민감도지표요청,	// == OPT10050
	[Description("OPT30009")]
	ELW등락율순위요청,
	[Description("OPT30010")]
	ELW잔량순위요청,
	[Description("OPT30011")]
	ELW근접율요청,
	[Description("OPT30012")]
	ELW종목상세정보요청,
	[Description("OPT40001")]
	ETF수익율요청,
	[Description("OPT40002")]
	ETF종목정보요청,
	[Description("OPT40003")]
	ETF일별추이요청,
	[Description("OPT40004")]
	ETF전체시세요청,
	//[Description("OPT40005")]
	//ETF일별추이요청,	// == OPT40003
	[Description("OPT40006")]
	ETF시간대별추이요청,
	[Description("OPT40007")]
	ETF시간대별체결요청,
	[Description("OPT40008")]
	ETF일자별체결요청,
	//[Description("OPT40009")]
	//ETF시간대별체결요청,	// == OPT40007
	//[Description("OPT40010")]
	//ETF시간대별추이요청,	// == OPT40006
	[Description("OPT50001")]
	선옵현재가정보요청,
	[Description("OPT50002")]
	선옵일자별체결요청,
	[Description("OPT50003")]
	선옵시고저가요청,
	[Description("OPT50004")]
	콜옵션행사가요청,
	[Description("OPT50005")]
	선옵시간별거래량요청,
	[Description("OPT50006")]
	선옵체결추이요청,
	[Description("OPT50007")]
	선물시세추이요청,
	[Description("OPT50008")]
	프로그램매매추이차트요청,
	[Description("OPT50009")]
	선옵시간별잔량요청,
	[Description("OPT50010")]
	선옵호가잔량추이요청,
	//[Description("OPT50011")]
	//선옵호가잔량추이요청,	// == OPT50010
	[Description("OPT50012")]
	선옵타임스프레드차트요청,
	[Description("OPT50013")]
	선물가격대별비중차트요청,
	//[Description("OPT50014")]
	//선물가격대별비중차트요청,	// == OPT50013
	[Description("OPT50015")]
	선물미결제약정일차트요청,
	[Description("OPT50016")]
	베이시스추이차트요청,
	//[Description("OPT50017")]
	//베이시스추이차트요청,	// == OPT50016
	[Description("OPT50018")]
	풋콜옵션비율차트요청,
	[Description("OPT50019")]
	선물옵션현재가정보요청,
	[Description("OPT50020")]
	복수종목결제원별시세요청,
	[Description("OPT50021")]
	콜종목결제월별시세요청,
	[Description("OPT50022")]
	풋종목결제월별시세요청,
	[Description("OPT50023")]
	민감도지표추이요청,
	[Description("OPT50024")]
	일별변동성분석그래프요청,
	[Description("OPT50025")]
	시간별변동성분석그래프요청,
	[Description("OPT50026")]
	선옵주문체결요청,
	[Description("OPT50027")]
	선옵잔고요청,
	[Description("OPT50028")]
	선물틱차트요청,
	[Description("OPT50029")]
	선물옵션분차트요청,
	[Description("OPT50030")]
	선물옵션일차트요청,
	[Description("OPT50031")]
	선옵잔고손익요청,
	[Description("OPT50032")]
	선옵당일실현손익요청,
	[Description("OPT50033")]
	선옵잔존일조회요청,
	[Description("OPT50034")]
	선옵전일가격요청,
	[Description("OPT50035")]
	지수변동성차트요청,
	[Description("OPT50036")]
	주요지수변동성차트요청,
	[Description("OPT50037")]
	코스피200지수요청,
	[Description("OPT50038")]
	투자자별만기손익차트요청,
	[Description("OPT50039")]
	투자자별포지션종합요청,
	//[Description("OPT50040")]
	//선옵시고저가요청,	// == OPT50003
	[Description("OPT50043")]
	주식선물거래량상위종목요청,
	[Description("OPT50044")]
	주식선물시세표요청,
	[Description("OPT50062")]
	선물미결제약정분차트요청,
	[Description("OPT50063")]
	옵션미결제약정일차트요청,
	[Description("OPT50064")]
	옵션미결제약정분차트요청,
	[Description("OPT50065")]
	풋옵션행사가요청,
	[Description("OPT50066")]
	옵션틱차트요청,
	[Description("OPT50067")]
	옵션분차트요청,
	[Description("OPT50068")]
	옵션일차트요청,
	[Description("OPT50071")]
	선물주차트요청,
	[Description("OPT50072")]
	선물월차트요청,
	[Description("OPT50073")]
	선물년차트요청,
	[Description("OPT90001")]
	테마그룹별요청,
	[Description("OPT90002")]
	테마구성종목요청,
	[Description("OPT90003")]
	프로그램순매수상위50요청,
	[Description("OPT90004")]
	종목별프로그램매매현황요청,
	[Description("OPT90005")]
	프로그램매매추이요청,
	[Description("OPT90006")]
	프로그램매매차익잔고추이요청,
	[Description("OPT90007")]
	프로그램매매누적추이요청,
	[Description("OPT90008")]
	종목시간별프로그램매매추이요청,
	[Description("OPT90009")]
	외국인기관매매상위요청,
	[Description("OPT90010")]
	차익잔고현황요청,
	//[Description("OPT90011")]
	//차익잔고현황요청,	// == OPT90010
	//[Description("OPT90012")]
	//대차거래내역요청,	// == OPT10067
	[Description("OPT90013")]
	종목일별프로그램매매추이요청,
	//[Description("OPT99999")]
	//대차거래상위10종목요청, // == OPT10069
	[Description("OPTFOFID")]
	선물전체시세요청,
	[Description("OPTKWFID")]
	관심종목정보요청,
	[Description("OPTKWINV")]
	관심종목투자자정보요청,
	[Description("OPTKWPRO")]
	관심종목프로그램정보요청,
	[Description("OPW00001")]
	예수금상세현황요청,
	[Description("OPW00002")]
	일별추정예탁자산현황요청,
	[Description("OPW00003")]
	추정자산조회요청,
	[Description("OPW00004")]
	계좌평가현황요청,
	[Description("OPW00005")]
	체결잔고요청,
	[Description("OPW00006")]
	관리자별주문체결내역요청,
	[Description("OPW00007")]
	계좌별주문체결내역상세요청,
	[Description("OPW00008")]
	계좌별익일결제예정내역요청,
	[Description("OPW00009")]
	계좌별주문체결현황요청,
	[Description("OPW00010")]
	주문인출가능금액요청,
	[Description("OPW00011")]
	증거금율별주문가능수량조회요청,
	[Description("OPW00012")]
	신용보증금율별주문가능수량조회,
	[Description("OPW00013")]
	증거금세부내역조회요청,
	[Description("OPW00014")]
	비밀번호일치여부요청,
	[Description("OPW00015")]
	위탁종합거래내역요청,
	[Description("OPW00016")]
	일별계좌수익률상세현황요청,
	[Description("OPW00017")]
	계좌별당일현황요청,
	[Description("OPW00018")]
	계좌평가잔고내역요청,
	[Description("OPW20001")]
	선물옵션청산주문위탁증거금가계산요청,
	[Description("OPW20002")]
	선옵당일매매변동현황요청,
	[Description("OPW20003")]
	선옵기간손익조회요청,
	[Description("OPW20004")]
	선옵주문체결내역상세요청,
	[Description("OPW20005")]
	선옵주문체결내역상세평균가요청,
	[Description("OPW20006")]
	선옵잔고상세현황요청,
	[Description("OPW20007")]
	선옵잔고현황정산가기준요청,
	[Description("OPW20008")]
	계좌별결제예상내역조회요청,
	[Description("OPW20009")]
	선옵계좌별주문가능수량요청,
	[Description("OPW20010")]
	선옵예탁금및증거금조회요청,
	[Description("OPW20011")]
	선옵계좌예비증거금상세요청,
	[Description("OPW20012")]
	선옵증거금상세내역요청,
	[Description("OPW20013")]
	계좌미결제청산가능수량조회요청,
	[Description("OPW20014")]
	선옵실시간증거금산출요청,
	[Description("OPW20015")]
	옵션매도주문증거금현황요청,
	[Description("OPW20016")]
	신용융자_가능종목요청,
	[Description("OPW20017")]
	신용융자_가능문의,
}