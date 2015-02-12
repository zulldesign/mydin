/********************************************************************
	BPCStretchSlider - класс резинового слайдера
	
	data - набор первоначальных данных
	position - номер первого элемента набора в ленте данных (если не указан, то первый (1))
	count - количество элементов в выборке
	active - активный элемент в выборке
	url - url для ajax - запроса новых превью-фоток
	recsPerPage - количество элементов на странице у компонента, обрабатывающего запрос на получение данных о фото
	нужен для расчета правильной страницы с фотками, которую нужно получить от компонента.

********************************************************************/
function BPCStretchSlider( position, count, active,url,recsPerPage) 
{
	if (count <= 0)
		return false;
	this.oSource = new BPCSourse([], position, count, recsPerPage);
	if (!this.oSource)
		return false;
	this.oSource.url = url;
	this.activeId = active;
	this.recsPerPage = recsPerPage;
	this.oSource.parentObject = this;
		this.oSource.OnAfterItemAdd = function()
	{
		//try
		//{

			var element_id = arguments[0][1]; 
			var item_data = this.Data[element_id]; 
			var item_id = item_data["id"]; 
			var item = document.getElementById('item_' + item_id); 

			if (!item)
			{
				var attrs={};
				attrs['class'] = "photo-slider-item";
				if ( item_id == active )
					attrs['class'] += ' photo-slider-item-active';
				var div = BX.create("DIV", {"props" : {"id" : "item_" + item_id}, "attrs" : attrs, 
					"html" : (
					'<table class="photo-slider-thumb" cellpadding="0">' + 
						'<tr>' + 
							'<td>' + 
								'<a href="' + item_data['photourl'] + '">' +( item_id == active ? "<div class=\"image\">":"")+
									this.parentObject.CreateItem(element_id) + ( item_id == active ? "</div>":"")+
								'</a>' + 
							'</td>' + 
						'</tr>' + 
					'</table>')}); 
					
				if (element_id < this.iFirstNumber)
				{
					var pointer = document.getElementById('item_' + this.Data[this.iFirstNumber]["id"]);
					if (pointer)
						pointer.parentNode.insertBefore(div, pointer); 
				}
				else
					this.parentObject.tape.appendChild(div); 
	
				pos = BX.pos(div); 

				this.parentObject.tape.__int_width += parseInt(pos['width']); 
				if (element_id < this.iFirstNumber)
				{
					this.parentObject.tape.style.left = (parseInt(this.parentObject.tape.style.left) - parseInt(pos['width'])) + 'px'; 
					this.parentObject.prev.className = this.parentObject.prev.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
				}
				else
				{
					this.parentObject.next.className = this.parentObject.next.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
				}
				

				this.parentObject.tape.style.width = this.parentObject.tape.__int_width+this.parentObject.params['step_size']-10+"px";
			}
		//} 
		//catch (e) { } 
	}

	this.oSource.iForceElementCount = 10;
	this.active = this.oSource.iFirstNumber;
	this.params = {'height' : 250, 'width' : 250, 'first_created' : 0, 'last_created' : 0, 'step_size' : 100};
	var __this = this;
	this.events = {}; 


	this.oSource.OnBeforeSendData = function() 
	{
		arguments[0][1]['package_id'] = this.parentObject.pack_id; 
		return arguments[0][1]; 
	}
	
	return true;
}
/** 
	CreateSlider Создает слайдер, перемещает курсор на активный элемент
*/
BPCStretchSlider.prototype.CreateSlider = function(data) 
{
	this.checkEvent('OnBeforeSliderCreate');
	this.oSource.load(data);
	for (var ii = this.oSource.iFirstNumber; ii < this.oSource.Data.length; ii++)
	{
		if (this.activeId == this.oSource.Data[ii]['id'])
			this.active = ii;
	}
	
	this.params['first_created'] = this.params['last_created'] = this.oSource.iFirstNumber; 
	for (var item_id = this.oSource.iFirstNumber; item_id < this.oSource.Data.length; item_id++)
	{
		this.params['last_created'] = item_id;
		
		var res = this.oSource.checkItem(item_id);
		if (!res || res == 'wait')
			return res;
		this.MakeItem(item_id, (this.active == item_id));
	}
	

	
	this.oSource.checkData(true,this.oSource.Data.length);// load next page to show in slider

	this.checkEvent('OnAfterSliderCreate');
	
	return true;
}
/** 
	MakeItem Создает элемент
	
	item_id - номер элемента
 	number - порядковый номер в окне
*/
BPCStretchSlider.prototype.MakeItem = function(item_id, active_id) 
{
	this.checkEvent('OnBeforeItemMake', item_id, active_id);
	this.ShowItem(item_id, active_id);
	this.checkEvent('OnAfterItemMake', item_id, active_id);
}
/** 
	ShowItem Отображает элемент (должна быть переопределена, так как привязана к объектам страницы)
	
	item_id - номер элемента
 	active_id - активный элемент
*/
BPCStretchSlider.prototype.ShowItem = function(item_id, active_id) 
{
}

/** 
	CreateItem - Создает элемент (внесена в этот класс, как наиболее часто повторяющаяся и практически неизменяющаяся, 
		но с классом никак не связана)

	item_id - номер эелемента
	
	Возвращает: объект или false
*/
BPCStretchSlider.prototype.CreateItem = function(item_id)
{
	var koeff = Math.min(this.params['width']/this.oSource.Data[item_id]['width'], this.params['height']/this.oSource.Data[item_id]['height']);
	var res = {'width' : this.oSource.Data[item_id]['width'], 'height' : this.oSource.Data[item_id]['height']};
	if (koeff < 1)
	{
		res['width'] = parseInt(this.oSource.Data[item_id]['width']*koeff);
		res['height'] = parseInt(this.oSource.Data[item_id]['height']*koeff);
	}
	__this_slider = this;
	
	var image = new Image();
	image.src = this.oSource.Data[item_id]['url']; 
	return '<img id="image_' + item_id + '" border="0" ' + 
		'onload="__this_slider.oSource.Data[this.id.replace(\'image_\', \'\')][\'loaded\'] = true; __this_slider.checkEvent(\'OnAfterItemLoad\', this);" ' + 
		'style="width:' + res['width'] + 'px;height:' + res['height'] + 'px;" ' + 
		'title="' + this.oSource.Data[item_id]['title'] + '" alt="' + this.oSource.Data[item_id]['title'] + '" ' + 
		'src="' + this.oSource.Data[item_id]['url'] + '" />';
}
/** 
	GoToNext - Перевод курсора на вправо
	
	Возвращает: true || false || 'wait'
*/
BPCStretchSlider.prototype.GoToNext = function()
{
	var pos_window = BX.pos(this.window); 
	var tape_right_width = parseInt(this.tape.__int_width) + parseInt(this.tape.style.left) - pos_window['width']+10; 

	var leftward = (tape_right_width > this.params['step_size'] ? this.params['step_size'] : tape_right_width); 
	if (leftward > 0)
	{
		this.tape.style.left = parseInt(parseInt(this.tape.style.left) - leftward) + 'px'; 
		this.prev.className = this.prev.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
	}

	if (this.oSource.Data.length <= this.oSource.iCountData && tape_right_width <= this.params['step_size'] )
		this.oSource.getData(this.oSource.Data.length, true);

	if (tape_right_width > this.params['step_size'])
		this.next.className = this.next.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
	else if (this.oSource.busy === true || this.oSource.Data.length < this.oSource.iCountData)
		this.next.className = this.next.className.replace("-enabled", "-wait").replace("-disabled", "-wait");
	else
		this.next.className = this.next.className.replace("-enabled", "-disabled").replace("-wait", "-disabled");
	
	return true;
}

/** 
	GoToPrev - Перевод курсора влево
	
	Возвращает: true || false || 'wait'
*/
BPCStretchSlider.prototype.GoToPrev = function()
{
	var tape_left_width = parseInt(this.tape.style.left) * (-1); 
	var rightward = (tape_left_width > this.params['step_size'] ? this.params['step_size'] : tape_left_width); 

	if (rightward > 0)
	{
		this.tape.style.left = parseInt(parseInt(this.tape.style.left) + rightward) + 'px'; 
		var pos_window = BX.pos(this.window); 
		var tape_right_width = parseInt(this.tape.__int_width) + parseInt(this.tape.style.left) - pos_window['width']; 
		if (tape_right_width > 0)
			this.next.className = this.next.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
	}
	
	if (this.oSource.iFirstNumber > 1 && rightward <= this.params['step_size'] )
		this.oSource.getData(this.oSource.iFirstNumber, false);

	if (tape_left_width > this.params['step_size'])
		this.prev.className = this.prev.className.replace("-disabled", "-enabled").replace("-wait", "-enabled");
	else if (this.oSource.busy === true || this.oSource.iFirstNumber > 1)
		this.prev.className = this.prev.className.replace("-enabled", "-wait").replace("-disabled", "-wait");
	else
		this.prev.className = this.prev.className.replace("-enabled", "-disabled").replace("-wait", "-disabled");

	return true;
}/**
	Проверка событий
*/
BPCStretchSlider.prototype.checkEvent = function()
{
	eventName = arguments[0];
	if (this.events[eventName]) { return this.events[eventName](arguments); } 
	if (this[eventName]) {return this[eventName](arguments); } 
	return true;
}

BPCStretchSlider.prototype.OnBeforeSliderCreate = function(image) 
{
	this.prev = document.getElementById('prev_' + this.pack_id); 
	this.next = document.getElementById('next_' + this.pack_id); 
	this.window = document.getElementById('slider_window_' + this.pack_id); 
	this.tape = this.window.firstChild; 
	this.oSource.parentObject = this; 
	this.__leftward = 0;
	this.__width = 0;
	this.tape.__int_width = 0;
	this.__active_element_founded = false; 
	__this = this; 
	if (this.window.addEventListener)
		this.window.addEventListener('DOMMouseScroll', __this.OnMouseWheel, false);
	__this = this;
	BX.bind(this.window, 'mousewheel', new Function('__this.OnMouseWheel(event);'));
}
BPCStretchSlider.prototype.OnMouseWheel = function(event)
{
	if (!event) 
		event = window.event;
	
	var wheelDelta = 0;
	
	if (event.wheelDelta) 
		wheelDelta = event.wheelDelta / 120;
	else if (event.detail) 
		wheelDelta = -event.detail / 3;
	BX.PreventDefault(event); 
	var steps = (wheelDelta > 0 ? wheelDelta : wheelDelta * (-1)); 
	for (var ii = 1; ii <= steps; ii++)
	{
		if (wheelDelta < 0)
			__this.GoToNext();
		else
			__this.GoToPrev();
	}
}
BPCStretchSlider.prototype.OnAfterSliderCreate = function()
{	
	this.tape.style.left = (this.__leftward > 0 ? ('-' + this.__leftward + 'px') : '0px'); 

	this.tape.__int_width = this.__width; 
	//this.tape.style.width = this.__width+100+"px";
	delete this.__leftward; 
	delete this.__width; 
	delete this.__active_element_founded; 
	
	__this = this;
	BX.bind(this.next, 'click', new Function('__this.GoToNext(arguments);'));
	BX.bind(this.prev, 'click', new Function('__this.GoToPrev(arguments);'));
}
BPCStretchSlider.prototype.OnAfterItemMake = function()
{

	arguments = arguments[0]; 
	var item_id = arguments[1]; 
	var is_active = (arguments[2] === false || arguments[2] === true ? arguments[2] : (arguments[2] === 'false' ? false : (arguments[2] === 'true' ? true : null))); 

	var item = document.getElementById('item_' + this.oSource.Data[item_id]['id']); 
	var pos = BX.pos(item); 
	this.__width += parseInt(pos['width']); 
	this.tape.style.width = this.__width+this.params['step_size']+"px";
	if (!this.__active_element_founded && (is_active === false || is_active === true))
	{
		if (is_active === false)
		{
			this.__leftward += parseInt(pos['width']); 
		}
		else
		{
			this.__active_element_founded = true; 
		}
	}
}

var SlideSlider = false;
var player = false;

function __show_slider(active, url, result)
{

    active = parseInt(active);
    var res = 0;
    if (active > 0)
    {
        for (var ii = 0; ii < result['elements'].length; ii++)
        {
            if (result['elements'][ii]['id'] == active)
            {
                res = ii;
                break;
            }
        }
    }
    res = ((res > 0 ? res : 0) + parseInt(result['start_number']));

    var speed_mixer = new BPCMixer(
        document.getElementById('bx_slider_mixers_border'), 
        document.getElementById('bx_slider_mixers_cursor'), 
        5, 
        {
            events : {
                BeforeSetCursor : function()
                {
                    arguments = arguments[0];
                    setTimeout(new Function("document.getElementById('bx_slider_speed').innerHTML = '" + arguments[1] + "'"), 10);
                    window.__photo_params['speed'] = arguments[1];
                    if (window.player && window.player.params['period'])
                        window.player.params['period'] = (window.__photo_params['speed'] + (window.__photo_params['effects'] ? 1.5 : 0));

                }
            }
        });
    document.getElementById('bx_slider_mixers_plus').onclick = function() {
        window.__photo_params['speed']++;
        speed_mixer.SetCursor(window.__photo_params['speed']);}
    document.getElementById('bx_slider_mixers_minus').onclick = function() {
        window.__photo_params['speed']--;
        speed_mixer.SetCursor(window.__photo_params['speed']);}

    if (!SlideSlider)
    {
        SlideSlider = new BPCSlider(
            result['elements'], 
            res, 
            result['elements_count'],
            result['start_number'], 
            result['recsPerPage']);
        if (url.length > 0)
        {
            SlideSlider.oSource.url = url;
        }
        SlideSlider.params = {'diff_size' : false};
        /**
            CreateItem - create picture
        */
        SlideSlider.CreateItem = function(item_id)
        {

            var koeff = Math.min(
                this.item_params['width'] / this.oSource.Data[item_id]['width'], 
                this.item_params['height'] / this.oSource.Data[item_id]['height']);
            var res = {
                'width' : this.oSource.Data[item_id]['width'], 
                'height' : this.oSource.Data[item_id]['height']};
            if (koeff < 1)
            {
                res['width'] = parseInt(res['width'] * koeff); 
                res['height'] = parseInt(res['height'] * koeff);
            }
            var image = new Image();
            __this_slider = this;
            image.id = 'image_' + item_id;
            image.onload = function(){
//                try
                {

                    var iNumber = parseInt(this.id.replace('image_', ''));
                    __this_slider.oSource.Data[iNumber]['loaded'] = true;
                    __this_slider.OnAfterItemLoad(this);
                }
//                catch (e) {}
            }

            image.style.width = res['width'] + 'px';
            image.style.height = res['height'] + 'px';
            image.style.visibility = 'hidden';
            image.title = image.alt = this.oSource.Data[item_id]['title'];
            image.src = this.oSource.Data[item_id]['url'];
            return image;
        }
        
        SlideSlider.OnAfterItemLoad = function(image) 
        {		
            var iWidthDiff = parseInt(image.style.width.replace('px', '')) + this.params['diff_size']['width'];
            var iHeightDiff = (parseInt(image.style.height.replace('px' , '')) + this.params['diff_size']['height']);
            var item_id = image.id.replace('image_', '');
            var div = document.createElement('div');
            div.className = "bx-slider-image-container";
            div.id = item_id;
            var styles = {
                'overflow' : 'hidden', 
                'width' : (iWidthDiff < oPhotoObjects['min_slider_width'] ? 
                    ((oPhotoObjects['min_slider_width'] - this.params['diff_size']['width'] ) + 'px') : 
                    image.style.width), 
                'height' : (iHeightDiff < oPhotoObjects['min_slider_height'] ? 
                    ((oPhotoObjects['min_slider_height'] - this.params['diff_size']['height']) + 'px') : 
                    image.style.height)}; 
            for (var ii in styles)
                div.style[ii] = styles[ii];

            if (iWidthDiff < oPhotoObjects['min_slider_width'] || iHeightDiff < oPhotoObjects['min_slider_height'])
            {
                var div_inner = div.appendChild(document.createElement('div'));
                div_inner.style.visibility = 'hidden';
                image.style.visibility = 'visible';
                if (iWidthDiff < oPhotoObjects['min_slider_width'])
                {
                    var tmp = oPhotoObjects['min_slider_width'] - iWidthDiff; 
                    div_inner.style.paddingRight = div_inner.style.paddingLeft = Math.ceil(tmp / 2) + 'px';
                    iWidthDiff = oPhotoObjects['min_slider_width'];
                }
                if (iHeightDiff < oPhotoObjects['min_slider_height'])
                {
                    var tmp = oPhotoObjects['min_slider_height'] - iHeightDiff; 
                    div_inner.style.paddingBotton = div_inner.style.paddingTop = Math.ceil(tmp / 2) + 'px';;
                    iHeightDiff = oPhotoObjects['min_slider_height'];
                }
                div_inner.appendChild(image);
            }
            else
            {
                div.appendChild(image);
            }

            this.oImageBox.appendChild(div);
            try {
                var res = this.oImageBox.lastChild.previousSibling; 
                while (res)
                {
                    this.oImageBox.removeChild(res);
                    res = this.oImageBox.lastChild.previousSibling; 
                }
            } catch(e) {}
            document.getElementById('bx_slider_container_header').style.visibility = 'visible';
            window.location.hash = 'photo' + this.oSource.Data[item_id]['id'];
            if (this.params['time']['resize'] > 0)
            {
                var params = jsUtilsPhoto.GetElementParams(this.oImageOuterBox);
                var wDiff = params['width'] - iWidthDiff;
                var hDiff = params['height'] - iHeightDiff;
                
                if (wDiff != 0 || hDiff != 0)
                {
                    new jsUtilsEffect.Scale(
                        this.oImageOuterBox, 
                        false, 
                        {
                            scaleXTo: (wDiff != 0 ? (iWidthDiff / params['width']) : 1.0), 
                            scaleYTo: (hDiff != 0 ? (iHeightDiff / params['height']) : 1.0), 
                            events: {
                                BeforeSetDimensions: function(obj, args)
                                {
                                    if (args[1]['height'])
                                    {
                                        var div = this.element.parentNode;
                                        if (!this.originParams['int_parent_top'])
                                        {
                                            this.originParams['int_parent_top'] = parseInt(div.style.top);
                                            this.originParams['int_height'] = parseInt(this.originParams['height']);
                                        }
                                        div.style.top = (this.originParams['int_parent_top'] + 
                                            parseInt((this.originParams['int_height'] - parseInt(args[1]['height'])) / 2)) + 'px';
                                    }
                                }
                            }, 
                            duration: this.params['time']['resize']}
                        ); 
                }
                __this_slider = this;
                setTimeout(new Function("__this_slider.ShowItemDetails(" + div.id + ");"), this.params['time']['resize'] * 1000);
            }
            else
            {
                this.oImageOuterBox.style.width = iWidthDiff + 'px';
                this.oImageOuterBox.style.height = iHeightDiff + 'px';
                this.oImageOuterBox.parentNode.style.top = (this.params['size']['y'] - parseInt(iHeightDiff / 2)) + 'px';
                this.ShowItemDetails(div.id);
            }
            
            this.oImageDataBox.style.width = iWidthDiff + 'px';
            this.PreloadItems(div.id);
            return true;
        }
        
        SlideSlider.ShowItemDetails = function(item_id) 
        {

            if (!this.oImageBox.firstChild || !this.oImageBox.firstChild.firstChild)
                return false;
            this.oImageBox.style.visibility = 'visible';
            this.oLoadBox.style.visibility = 'hidden';
            var template = window.__photo_params['template']
            var template_additional = window.__photo_params['template_additional'];

            var template_vars = {
                title : /\#title\#/gi,
                origtext:/\#origtext#/gi,  
                description : /\#description\#/gi, 
                url: /\#url\#/gi};

            for (var key in template_vars)
            {
                var replacement = (this.oSource.Data[item_id][key] ? this.oSource.Data[item_id][key] : ''); 
                replacement = ((replacement + '') == "0" ? '' : replacement); 
                template = template.replace(template_vars[key], replacement); 
                template_additional = template_additional.replace(template_vars[key], replacement); 
            }
            
            
            var tt = this.oImageDataBox.getElementsByTagName('div');
            var bFounded = false;
            for (var ii = 0; ii < tt.length; ii++)
            {
                if (tt[ii].id == 'bx_caption')
                {
                    tt[ii].innerHTML = template;
                    if (bFounded) { break ;}
                    bFounded = true;
                }
                if (tt[ii].id == 'bx_caption_additional')
                {
                    tt[ii].innerHTML = template_additional;
                    if (bFounded) { break ;}
                    bFounded = true;
                }
            }
            
            
            if (document.getElementById('element_number'))
                document.getElementById('element_number').innerHTML = item_id;
            if (this.params['time']['data'] <= 0)
            {
                this.oImageBox.firstChild.firstChild.style.visibility = 'visible';
                this.oImageDataBox.style.display = 'block';
            }
            else
            {
                new jsUtilsEffect.Transparency(this.oImageBox.firstChild.firstChild, {'duration' : this.params['time']['data'] * 0.3}); 
                new jsUtilsEffect.Untwist(this.oImageDataBox, {'duration' : this.params['time']['data']}); 
            }

            if (document.getElementById('bx_slider_datacontainer').style.display == 'none')
                document.getElementById('bx_slider_datacontainer').style.display = 'block';
            this.oNavNext.style.height = this.oNavPrev.style.height = this.oImageOuterBox.style.height;

            var __this = this;
            setTimeout(function() {
                __this.status = 'ready'; 
                __this.oNavNext.style.display = __this.oNavPrev.style.display = 'block';
            }, (this.params['time']['data'] > 0 ? this.params['time']['data'] * 1000 : 100)); 
        }
        
        SlideSlider.ShowItem = function(item_id, number)
        {

            if (this.status == 'inprogress')
                return false;
            this.status = 'inprogress';
            // hide image info
            this.oImageBox.style.visibility = 'hidden';
            this.oNavPrev.style.display = this.oNavNext.style.display = 'none';
            if (this.params['time']['resize'] > 0)
            {
                this.oLoadBox.style.visibility = 'visible';
                try    {
                    var oChildNodes = this.oImageBox.childNodes;
                    if (oChildNodes && oChildNodes.length > 0)
                    {
                        for (var jj = 0; jj < oChildNodes.length; jj++)
                            this.oImageBox.removeChild(oChildNodes[jj]);
                    }
                } catch(e) {}
            }
            if (this.params['time']['data'] > 0)
            {
                this.oImageDataBox.style.display = 'none';
            }
            
            this.CreateItem(item_id);
            
            return true;
        }        
    }
    
    SlideSlider.active = res;
    SlideSlider.oLoadBox = document.getElementById('bx_slider_content_loading');
    SlideSlider.oImageOuterBox = document.getElementById('bx_slider_container_outer');
    SlideSlider.oImageBox = document.getElementById('bx_slider_content_item');
    SlideSlider.oImageDataBox = document.getElementById('bx_slider_datacontainer_outer');
    SlideSlider.oNavPrev = document.getElementById('bx_slider_nav_prev');
    SlideSlider.oNavNext = document.getElementById('bx_slider_nav_next');
    
    SlideSlider.params['diff_size'] = {
        'width' : (SlideSlider.oImageOuterBox.offsetWidth - SlideSlider.oImageBox.offsetWidth), 
        'height' : 37};
    SlideSlider.params['size'] = {
        'x' : 'center', 
        'y' : (parseInt(SlideSlider.oImageOuterBox.parentNode.style.top) + parseInt(SlideSlider.oImageOuterBox.offsetHeight / 2))};
    
    var ImageRectangle = GetImageWindowSize();
    SlideSlider.params['time'] = {
        'resize' : 0,/*(window.__photo_params['effects'] ? 0.5 : 0), */
        'data' : 0/*(window.__photo_params['effects'] ? 0.8 : 0)*/};
    SlideSlider.item_params = {
        'width' : (ImageRectangle['width'] - SlideSlider.params['diff_size']['width']), 
        'height' : (ImageRectangle['height'] - SlideSlider.params['diff_size']['height'])};
    SlideSlider.item_params['width'] = (SlideSlider.item_params['width'] < oPhotoObjects['min_slider_width'] ? oPhotoObjects['min_slider_width'] : SlideSlider.item_params['width']);
    SlideSlider.item_params['height'] = (SlideSlider.item_params['height'] < oPhotoObjects['min_slider_height'] ? oPhotoObjects['min_slider_height'] : SlideSlider.item_params['height']);

    SlideSlider.ShowSlider();
    
    document.getElementById('element_count').innerHTML = result['elements_count'];
    
    BPCPlayer.prototype.OnStopPlay = function()
    {
        if (document.getElementById('bx_slider_nav_pause'))
            document.getElementById('bx_slider_nav_pause').id = 'bx_slider_nav_play';
    }
    BPCPlayer.prototype.OnStartPlay = function()
    {
        if (document.getElementById('bx_slider_nav_play'))
            document.getElementById('bx_slider_nav_play').id = 'bx_slider_nav_pause';
    }
    
    window.player = new BPCPlayer(SlideSlider);
    
    if (player)
    {
        player.params = {
            'period' : (window.__photo_params['speed'] + (window.__photo_params['effects'] ? 1.5 : 0)), 
            'status' : 'paused'};
        window.__checkKeyPress = function(e)
        {
            if (SlideSlider && SlideSlider.status != 'inprogress')
            {
                __this_player = player;
                player.checkKeyPress(e);
            }
        }
        jsUtils.addEvent(document, "keypress", __checkKeyPress);
    }
    else
    {
        document.getElementById("bx_slider_content_item").innerHTML = '<div class="error">Error. <a href="' + 
            window.location.href + '">Refresh</a>. </div>';
    }
}

bPhotoSliderLoad = true;