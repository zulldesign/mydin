﻿


function BPCMixer(field, cursor, gradation, params)
{

    this.field = field;
    this.cursor = cursor;
    params = (params ? params : {});
    this.minus = (params['minus'] ? params['minus'] : false);
    this.plus = (params['plus'] ? params['plus'] : false);
    this.events = (params['events'] ? params['events'] : {});

    this.gradation = (gradation > 2 ? gradation : 7);
    this.current = 1;
    
    var _this = this;
    
    this.field.onmousedown = function(e) {
        if(!e) { e = window.event; } 
        _this.SetCursor(e.clientX + document.body.scrollLeft, true);
    }; 
    this.cursor.onmousedown = function(e) {
        _this.StartDrag(e, this);}
    this.cursor.onclick = function(e) {
        return false;}
    if (this.minus)
    {
        this.minus.onclick = function(e){
            if (_this.current > 1)
                _this.SetCursor(_this.current - 1);
        }
    }
    if (this.plus)
    {
        this.plus.onclick = function(e){
            if (_this.current < _this.gradation)
                _this.SetCursor(_this.current + 1);
        }
    }
    
    this.Init = function()
    {
        if (!this.params)
            this.params = jsUtilsPhoto.GetElementParams(this.field);
        this.current = (this.current > 0 ? this.current : 1);
        if (!this.cursor_params)
            this.cursor_params = jsUtilsPhoto.GetElementParams(this.cursor);
        this.params['real_width'] = this.params['width'] - this.cursor_params['width'];
        this.grating_period_px = Math.round(this.params['real_width'] / (this.gradation - 1));
        this.grating_period = Math.round(100 / (this.gradation - 1));
    }
    
    this.StartDrag = function(e, div)
    {
        if(!e)
            e = window.event;
        this.x = e.clientX + document.body.scrollLeft;
        this.y = e.clientY + document.body.scrollTop;
        this.floatDiv = div;

        jsUtils.addEvent(document, "mousemove", this.MoveDrag);
        document.onmouseup = this.StopDrag;
        if(document.body.setCapture)
            document.body.setCapture();

        document.onmousedown = jsUtils.False;
        var b = document.body;
        b.ondrag = jsUtils.False;
        b.onselectstart = jsUtils.False;
        b.style.MozUserSelect = _this.floatDiv.style.MozUserSelect = 'none';
    }

    this.StopDrag = function(e)
    {
        if(document.body.releaseCapture)
            document.body.releaseCapture();

        jsUtils.removeEvent(document, "mousemove", _this.MoveDrag);
        document.onmouseup = null;

        this.floatDiv = null;

        document.onmousedown = null;
        var b = document.body;
        b.ondrag = null;
        b.onselectstart = null;
        b.style.MozUserSelect = _this.floatDiv.style.MozUserSelect = '';
        b.style.cursor = '';
    }

    this.MoveDrag = function(e)
    {
        var x = e.clientX + document.body.scrollLeft;
        _this.SetCursor(x, true);
    }
    
    this.SetCursor = function(position, need_calc)
    {
        this.Init();
        if (need_calc == true)
        {
            this.params = jsUtilsPhoto.ObjectsMerge(this.params, jsUtils.GetRealPos(this.field));
            position = parseInt(position);
            position = Math.round((position - this.params['left']) / this.grating_period_px) + 1; 
        }
        
        position = (position < 1 ? 1 : (position > this.gradation ? this.gradation : position));
        
        if (this.current != position)
        {
            this.current = position;
            
            this.checkEvent('BeforeSetCursor', this.current);
            
            if (this.current <= 1)
                this.cursor.style.left = '0';
            else
            {
                var position = Math.round(((this.params['real_width'] * (this.current - 1) / (this.gradation - 1)) / this.params['width']) * 100);
                this.cursor.style.left = position + '%';
            }
            this.cursor.parentNode.style.display = 'none'; 
            this.cursor.parentNode.style.display = 'block'; 
            this.checkEvent('AfterSetCursor', this.current);
        }
    }
    this.checkEvent = function()
    {
        eventName = arguments[0];
        if (this[eventName]) {return this[eventName](arguments); } 
        if (this.events[eventName]) {return this.events[eventName](arguments, this); } 
        return true;
    }
}


/**
	Есть три класса: BPCSourse, BPCSlider, BPCPlayer
	
	BPCSourse - класс "источника данных", отвечающий за сбор необходи-
	мой информации. 
	
	data - набор первоначальных данных
	id - номер первого элемента набора в ленте данных (если не указан, то первый (1))
	count - количество элементов в ленте данных (если не известно, то 0)
*/
function BPCSourse(data, id, count,recsPerPage) {
	id = parseInt(id);
	id = (id > 0 ? id : 1);
	count = parseInt(count);
	this.Data = new Array(id); // массив данных "порядковый номер" => данные
	this.iCountData = (count > 0 ? count : 0); // максимальное количество элементов
	this.iFirstNumber = id; // первый элемент заполненной области
	this.iForceElementCount = 4; // число пограничных НЕ пустых элементов 
	this.loaded = false; // индикатор загрузки апплета
	this.arParams = {'attempt' : {}}; // служебная информация
	this.recsPerPage =parseInt(recsPerPage);
	this.events = {}; 
	this.load(data);
	this.url = false;
	return this.loaded;
}

BPCSourse.prototype.load = function (data)
{
if (data['start_number'] > 0)
		{
			if (data['elements'] && data['elements'].length > 0)
			{

				if (this.Data.length < data['start_number'])
				{
					var res = this.Data.length;
					for (var ii = res; ii < data['start_number']; ii++)
						this.Data[ii] = false;
				}
				for (var ii = 0; ii < data['elements'].length; ii++)
				{
					var jj = data['start_number'] + ii;
					if ((!this.Data[jj] || this.Data[jj] == null) && this.checkEvent('OnBeforeItemAdd', data['elements'][ii], jj))
					{
						this.Data[jj] = data['elements'][ii];
						this.checkEvent('OnAfterItemAdd', jj);
					}
				}
			}
			if (data['start_number'] < this.iFirstNumber)
				this.iFirstNumber = data['start_number'];
		}


		this.loaded = true;
	//} catch (e){}
}
/**
	getData - Возвращает набор данных
	
	iLastNumber - порядковый номер в массиве 
	bDirection - направление движения (true - вправо, false - влево)
	
	Возвращает: false | 'wait' | набор данных
*/

BPCSourse.prototype.getData = function(iLastNumber, bDirection) 
{
	//debugger;
    if (!this.loaded)
        return false;
    bDirection = !!bDirection; // true = next, false = prev
    iLastNumber = parseInt(iLastNumber);
//     если у нас не корректный номер 
    if (iLastNumber < 1)
        return false;
//    если задано максимальное количество и номер больше 
//    этого количества, то тогда говорим, что отдать ничего не можем
    else if (this.iCountData > 0 && iLastNumber > this.iCountData)
        return false;

//    если нам есть что отдать
    if (bDirection && this.Data[iLastNumber] ) 
    {
		
        if ((this.Data.length - iLastNumber) < this.iForceElementCount){
			
            this.checkData(bDirection,iLastNumber);
            }
        return this.Data.slice(iLastNumber);
    }
    else if (!bDirection && this.Data[iLastNumber])
    {
        if ((iLastNumber - this.iFirstNumber) < this.iForceElementCount)
            this.checkData(bDirection,iLastNumber);
          
        return this.Data.slice(this.iFirstNumber, iLastNumber);
    }
    
    return this.checkData(bDirection,iLastNumber);
}

/**
	checkData - Проверяет наличие данных. Если данных нет, то посылает запрос
	
	bDirection - направление движения (true - вправо, false - влево)
	
	Возвращает: false | 'wait' | true
*/
BPCSourse.prototype.checkData = function(bDirection,iLastNumber) 
{

	bDirection = !!bDirection;
	if (!this.loaded)
		return false;
	else if (this.busy == true)
		return 'wait';

	 else if ((bDirection && this.iCountData > 0 && (this.Data.length - 1) >= this.iCountData) && this.Data[iLastNumber] ||
	(!bDirection && this.iFirstNumber <= 1  && this.Data[iLastNumber]))
	 return true; 
//	else if ((bDirection && this.iCountData > 0 && /*this.Data[iLastNumber]*/(this.Data.length - 1) >= this.iCountData) || 
	//	(!bDirection && this.Data[this.iFirstNumber] && this.Data[iLastNumber] ))
	//	return true;
	else if (this.busy != true && !this.checkSendData(bDirection,iLastNumber) && this.busy != true)
	{
		this.addData(bDirection, (bDirection ? 
			'{"status" : "end"}' : 
			'{"start_number" : ' + (this.iFirstNumber - 1) + ', "elements" : ' + 
				'{"src" : "/bitrix/components/bitrix/photogallery.detail.list/templates/slide_show/images/error.gif"}}'));
	}
	
	__this_source = this;
	setTimeout(new Function("__this_source.sendData(" + (bDirection ? "true" : "false")+","+ iLastNumber + ")"), 100);
	return 'wait';
}
/**
	checkSendData - Проверяет количество поппыток отправки запросов с одинаковыми условиями
	
	bDirection - направление движения (true - вправо, false - влево)
	
	Возвращает: false | true
*/
BPCSourse.prototype.checkSendData = function(bDirection)
{
	if (this.busy == true)
		return false;
	bDirection = !!bDirection;
	var res = (bDirection ? 'next:' + this.Data.length : 'prev:' + this.Data.length);
	this.arParams['attempt'][res] = (this.arParams['attempt'][res] ? this.arParams['attempt'][res] : 0);
	if (parseInt(this.arParams['attempt'][res]) > 20)
		return false;
	this.arParams['attempt'][res]++;
	return true;
}
/**
	sendData - Отправляет запрос на получение данных
	
	bDirection - направление движения (true - вправо, false - влево)
	
	Возвращает: false | 'wait' | true
*/
BPCSourse.prototype.sendData = function(bDirection,iLastNumber)
{
	//debugger;
	if (this.busy == true)
		return 'wait';
	this.busy = true;
	
	bDirection = !!bDirection;

	var current = (bDirection ? this.Data.slice(-1) : this.Data.slice(this.iFirstNumber, this.iFirstNumber+1));
	var url;
	if (!this.url) { url = window.location.href; } 
	else url = this.url;

	var __this_source = this;

	var page;
	page =Math.floor((iLastNumber > 1 ? iLastNumber+1 : iLastNumber) / this.recsPerPage);
	if (bDirection || page==0) page++;

	url = url.replace('bxpagenumber',page);

	var request = Bitrix.HttpUtility.createXMLHttpRequest();
	request.open("GET", url, true);
	var self = this;

	request.onreadystatechange = function(){
		if(request.readyState != 4) return;
		var r = "", 
			txt = request.responseText;

		__this_source.addData(bDirection, txt);
	};
	request.send(); 

}

BPCSourse.prototype.checkSameDataReceived = function(data)
{
	if ( data.length==0)
		return true;

	for ( var i = 0;i < this.Data.length; i++)
		if ( this.Data[i] ){
		if ( this.Data[i].id == data[0].id )
			return true;
		}
	return false;
}

/**
	addData - Добавляет данные в массив
	
	bDirection - направление движения (true - вправо, false - влево)
	data - данные
*/

BPCSourse.prototype.addData = function(bDirection, data)
{

	bDirection = !!bDirection;
	try
	{

		eval("var result=" + data + ";");
		result['start_number'] = parseInt(result['start_number']);

		if (this.checkSameDataReceived(result['elements'])){
		
			this.checkEvent('OnAfterSendData');
			this.busy = false;
			return;
		}
		if (result['start_number'] > 0)
		{
			if (result['elements'] && result['elements'].length > 0)
			{
				if (this.Data.length < result['start_number'])
				{
					var res = this.Data.length;
					for (var ii = res; ii < result['start_number']; ii++)
						this.Data[ii] = false;
				}
				for (var ii = 0; ii < result['elements'].length; ii++)
				{
					var jj = result['start_number'] + ii;
					if ((!this.Data[jj] || this.Data[jj] == null) && this.checkEvent('OnBeforeItemAdd', result['elements'][ii], jj))
					{
						this.Data[jj] = result['elements'][ii];
						this.checkEvent('OnAfterItemAdd', jj);
					}
				}
			}
			if (result['start_number'] < this.iFirstNumber)
				this.iFirstNumber = result['start_number'];
		}
		
		//if (result['start_number'] <= 0 || !(result['elements'] && result['elements'].length > 0) || 
		//	result['status'] == 'end')
		//{
		//	this.iCountData = (this.Data.length - 1);
		//}
	}
	catch (e) {}
	this.checkEvent('OnAfterSendData');
	this.busy = false;
}
/**
	checkItem - Проверяет корректность элемента в массиве
	
	item_id - порядковый номер в массиве 
	bDirection - направление движения (true - вправо, false - влево)
	
	Возвращает: false | 'wait' | набор данных
*/
BPCSourse.prototype.checkItem = function(item_id, bDirection)
{
	return true;
}
/**
	Проверка событий
*/
BPCSourse.prototype.checkEvent = function()
{
	eventName = arguments[0];
//	if (arguments && arguments.shift) {arguments.shift()};
	if (this.events[eventName]) { return this.events[eventName](arguments); } 
	if (this[eventName]) { return this[eventName](arguments); } 
	return true;
}
/********************************************************************
	BPCSlider - класс слайдера
	
	data - набор первоначальных данных
	active - активных элемент массива
	count - количество элементов в ленте данных (если не известно, то 0)
	position - номер первого элемента набора в ленте данных (если не указан, то первый (1))
	
********************************************************************/
function BPCSlider(data, active, count, position,recsPerPage) 
{

	if (count <= 0)
		return false;
	this.oSource = new BPCSourse(data, position, count,recsPerPage);
	if (!this.oSource)
		return false;
	this.windowsize = 1;
	this.oSource.iForceElementCount = this.windowsize * 3;
	this.active = this.oSource.iFirstNumber;
	this.item_params = {'width' : 800, 'height' : 600};
	this.events = {}; 
	for (var ii = this.oSource.iFirstNumber; ii < this.oSource.Data.length; ii++)
	{
		if (active == this.oSource.Data[ii]['id'])
			this.active = ii;
	}
}
/** 
	ShowSlider - инициализация слайдера
	
	Возвращает: true || false || 'wait'
*/
BPCSlider.prototype.ShowSlider = function(data) 
{

	
	for (var ii = 1; ii <= this.windowsize; ii++) 
	{
		var item_id = (this.active - 1 + ii);
		
		if (!this.oSource.Data[item_id])
		{
			var res = this.oSource.checkItem(item_id);
			if (!res || res == 'wait')
				return res;
		}
		if (!this.oSource.Data[item_id] || (this.oSource.Data[item_id]['loaded'] != true && !this.checkEvent('OnBeforeItemShow', item_id)))
		{
			return 'wait';
		}
	}
	
	for (var ii = 0; ii < this.windowsize; ii++)
	{
		var item_id = (this.active + ii);
		this.MakeItem(item_id, (ii + 1));
	}
	return true;
}
/** 
	MakeItem Создает элемент
	
	item_id - номер эелемента
 	number - порядковый номер в окне
*/
BPCSlider.prototype.MakeItem = function(item_id, number) 
{
	this.checkEvent('OnBeforeItemShow', item_id);
	this.ShowItem(item_id, number);
}
/** 
	ShowItem Отображает элемент (должна быть переопределена, так как привязана к объектам страницы)
	
	item_id - номер эелемента
 	number - порядковый номер в окне
*/
BPCSlider.prototype.ShowItem = function(item_id, number) 
{
}

/** 
	CreateItem - Создает элемент (внесена в этот класс, как наиболее часто повторяющаяся и практически неизменяющаяся, 
		но с классом никак не связана)

	item_id - номер эелемента
	
	Возвращает: объект или false
*/
BPCSlider.prototype.CreateItem = function(item_id)
{
	var koeff = Math.min(this.item_params['width']/this.oSource.Data[item_id]['width'], this.item_params['height']/this.oSource.Data[item_id]['height']);
	var res = {'width' : this.oSource.Data[item_id]['width'], 'height' : this.oSource.Data[item_id]['height']};
	if (koeff < 1)
	{
		res['width'] = parseInt(this.oSource.Data[item_id]['width']*koeff);
		res['height'] = parseInt(this.oSource.Data[item_id]['height']*koeff);
	}
	
	var div = document.createElement('div');
	div.className = "bx-slider-image-container";
	div.style.overflow = 'hidden';
//	div.style.visibility = 'hidden';
	div.style.width = res['width'] + "px";
	div.style.height = res['height'] + "px";
	div.id = this.oSource.Data[item_id]['id'];
	
	var image = new Image();
	image.id = 'image_' + item_id;
	__this_slider = this;
	image.onload = function(){
		__this_slider.oSource.Data[this.id.replace('image_', '')]['loaded'] = true;
		__this_slider.checkEvent('OnAfterItemLoad', this);
	}
	image.style.width = res['width'] + "px";
	image.style.height = res['height'] + "px";
	image.title = image.alt = this.oSource.Data[item_id]['title'];
	div.appendChild(image);
	image.src = this.oSource.Data[item_id]['src'];
	return div;
}

/** 
	PreloadItems - предварительная загрузка
	
	item_id - номер эелемента
*/
BPCSlider.prototype.PreloadItems = function(item_id) 
{
	item_id = parseInt(item_id);
	var images = new Array();
	var res = [(item_id - 1), (item_id + 1)];
	for (var jj in res)
	{
		var ii = res[jj];
		if (this.oSource.Data[ii] && !this.oSource.Data[ii]['loaded'])
		{
			images[ii] = new Image();
			images[ii].id = 'preload_image_' + ii;
			images[ii].onload = function(){ __this_slider.oSource.Data[this.id.replace('preload_image_', '')]['loaded'] = true; };
			images[ii].src = this.oSource.Data[ii]['url'];
		}
	}
	return true;
}

/** 
	GoToNext - Перевод курсора на вправо
	
	Возвращает: true || false || 'wait'
*/
BPCSlider.prototype.GoToNext = function()
{
	res = this.oSource.getData((this.active + this.windowsize), true);
	if (!res || res == 'wait')
		return res;
	this.active++;
	return true;
}
/** 
	GoToNext - Перевод курсора в конец
	
	Возвращает: true || false || 'wait'
*/
BPCSlider.prototype.GoToLast = function()
{
	res = this.oSource.getData((this.oSource.iCountData - this.windowsize + 1), true);
	if (!res || res == 'wait')
		return res;
	this.active = (this.oSource.iCountData - this.windowsize + 1);
	return true;
}
/** 
	GoToNext - Перевод курсора влево
	
	Возвращает: true || false || 'wait'
*/
BPCSlider.prototype.GoToPrev = function()
{
	res = this.oSource.getData((this.active - 1), false);
	if (!res || res == 'wait')
	{
		return res;
	}
	this.active--;
	return true;
}
/** 
	GoToNext - Перевод курсора в начало
	
	Возвращает: true || false || 'wait'
*/
BPCSlider.prototype.GoToFirst = function()
{
	res = this.oSource.getData(1, false);
	if (!res || res == 'wait')
		return res;
	this.active = 1;
	return true;
}
/**
	Проверка событий
*/
BPCSlider.prototype.checkEvent = function()
{
	eventName = arguments[0];
	if (this.events[eventName]) { return this.events[eventName](arguments); } 
	if (this[eventName]) {return this[eventName](arguments); } 
	return true;
}

BPCSlider.prototype.voteForPhoto = function(id,sign)
{
	var item=null;
	for ( var i = 0; i < this.oSource.Data.length;i++){
		if ( !this.oSource.Data[i] ) continue;
		if ( this.oSource.Data[i].id == id ){ 
			item = this.oSource.Data[i];
			break;
			}	
	}
	if ( !item ) return false;
	if ( !item.voting_allowed) 
		return false;
	var url = item.voting_url;
	var request = Bitrix.HttpUtility.createXMLHttpRequest();
	url+="&sign="+sign;
	request.open("GET", url, true);
	
	var self = this;

	request.onreadystatechange = function(){
		if(request.readyState != 4) return;
		var data;
		var txt = request.responseText;
		try{
			data = eval("("+txt+")");
		}
		catch(e){}
		if ( data )
		{

			self.oSource.Data[i].voting_result = data.total_value;
			self.oSource.Data[i].voting_allowed = false;
			self.refreshVoting(data.total_value,false);
		}
	};
	request.send(); 
	return false;
		
}

BPCSlider.prototype.refreshVoting = function(value,isVotingAllowed)
{
	var voteContainer = document.getElementById("bx-voting-container");
	if (voteContainer)
		voteContainer.setAttribute("class",isVotingAllowed ?  "rating-vote":"rating-vote rating-vote-disabled");
	var voteResultContainer = document.getElementById("bx-voting-result-container");
	if (voteResultContainer){
		if ( voteResultContainer.innerText )
			voteResultContainer.innerText = value;
		else if ( voteResultContainer.textContent )
			voteResultContainer.textContent = value;
		voteResultContainer.setAttribute("class",value >= 0 ? "rating-vote-result rating-vote-result-plus": "rating-vote-result rating-vote-result-minus");
	}	
}

/********************************************************************
	BPCPlayer - класс плейера
	
	oSlider - объект слайдера
********************************************************************/
BPCPlayer = function(oSlider)
{
	if (!oSlider)
		return false;
	this.oSlider = oSlider;
	this.events = {};
	this.params = {
		'period' : 5, 
		'status' : 'paused'};
}
/** 
	step - совершает шаг (вперед, назад, конец, начало)
	
	Возвращает: false || 'wait' || набор данных
*/
BPCPlayer.prototype.step = function(name_step)
{
	var res = '';
	this.stop();
	
	if (name_step == 'next')
	{
		res = this.oSlider.GoToNext();
		if (!res)
			res = this.oSlider.GoToFirst();
	}
	else if (name_step == 'prev')
	{
		res = this.oSlider.GoToPrev();
		if (!res)
			res = this.oSlider.GoToLast();
	}
	else if (name_step == 'last')
		res = this.oSlider.GoToLast();
	else
		res = this.oSlider.GoToFirst();
		
	if (res == 'wait')
	{
		this.checkEvent('OnWaitItem');
		__this_player = this;
		setTimeout(new Function("__this_player.step('" + name_step + "');"), 200);
	}
	else if (res != false)
	{
		this.checkEvent('OnShowItem');
		this.oSlider.ShowSlider();
	}
	return res;
}
/** 
	play - слайд-шоу (только влево)
	
	Возвращает: false || 'wait' || набор данных
*/
BPCPlayer.prototype.play = function(status)
{
	status = (status == true ? true : false);
	this.checkEvent('OnStartPlay');	
	if (this.params['period'] <= 0 || this.params['status'] != 'play')
	{
		this.checkEvent('OnStopPlay');
		return false;
	}
	// если это первый шаг, то должна быть задержка
	else if (!status)
	{
		__this_player = this;
		setTimeout(function(){__this_player.play(true);}, this.params['period'] * 1000);
	}
	else
	{
		// передвигаем курсор
		var res = this.oSlider.GoToNext();
		// если мы достигли конца, то останавливаем слайд-шоу
		if (res == false) 
		{
			this.checkEvent('OnStopPlay');
			return false;
		}
		// если данные не подгрузились, то обращаемся через 200 милисек.
		else if (res == 'wait') 
		{
			this.checkEvent('OnWaitItem');
			__this_player = this;
			setTimeout(function(){__this_player.play(true);}, 200);
		}
		else
		{
			this.checkEvent('OnShowItem');
			__this_player = this;
			// если фото была показана, то запускаем след. шаг
			if (this.oSlider.ShowSlider({'slideshow' : true}))
			{
				setTimeout(function(){__this_player.play(true);}, this.params['period'] * 1000);
			}
			// если нет, то передвигаем курсор назад, ждем, когда фотка загрузится.
			else
			{
				this.oSlider.GoToPrev();
				setTimeout(function(){__this_player.play(true);}, 200);
			}
		}
		return res;
	}
}
/** 
	stop - останавливает слайд-шоу
*/
BPCPlayer.prototype.stop = function(cycle)
{
	this.params['status'] = 'paused';
	this.checkEvent('OnStopPlay');
}
/**
	Проверка событий
*/
BPCPlayer.prototype.checkEvent = function()
{
	eventName = arguments[0];
	if (this.events[eventName]) { return this.events[eventName](arguments); } 
	if (this[eventName]) { return this[eventName](arguments); } 
	return true;
}
/**
	Проверка нажатий клавиш
*/
BPCPlayer.prototype.checkKeyPress = function(e)
{
	if(!e) e = window.event
	if(!e) return;
	if(e.keyCode == 39)
		__this_player.step('next');
	else if(e.keyCode == 37)
		__this_player.step('prev');
}

window.bPhotoPlayerLoad = true;