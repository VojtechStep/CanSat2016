/// <reference path="../typings/main.d.ts" />

var $ContentViewport;
var $Content;
var $Window;

var $NavBarHeightReference;
var $FooterHeightReference;

var $NewsLink;
var $AboutCompLink;
var $EventsLink;
var $ContactsLink;
var $AboutUsLink;

$(document).ready(() => {

    $ContentViewport = $("div.ContentViewport");
    $Content = $("div.ContentViewport > div.Content");
    $FooterHeightReference = $("ul.FooterStack");
    $Window = $(window);

    $NavBarHeightReference = $("ul.HeaderWrapper");

    $NewsLink = $("#NewsLink");
    $AboutCompLink = $("#AboutCompLink");
    $EventsLink = $("#EventsLink");
    $ContactsLink = $("#ContactsLink");
    $AboutUsLink = $("#AboutUsLink");

    SetViewport();

    $.ajaxSetup({ cache: false });

    $NewsLink.click(() => {
        $Content.animate({
            "margin-left": $Window.outerWidth() * 0 * -1
        }, 500);
        if ($Window.outerWidth() < 730)
            $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $AboutCompLink.click(() => {
        $Content.animate({
            "margin-left": $Window.outerWidth() * 1 * -1
        }, 500);
        if ($Window.outerWidth() < 730)
            $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $("#EventsLink").click(() => {
        $Content.animate({
            "margin-left": $Window.outerWidth() * 2 * -1
        }, 500);
        if ($Window.outerWidth() < 730)
            $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $ContactsLink.click(() => {
        $Content.animate({
            "margin-left": $Window.outerWidth() * 3 * -1
        }, 500);
        if ($Window.outerWidth() < 730)
            $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $AboutUsLink.click(() => {
        $Content.animate({
            "margin-left": $Window.outerWidth() * 4 * -1
        }, 500);
        if ($Window.outerWidth() < 730)
            $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $Content.attrchange({
        trackValues: true,
        callback: evnt => {
            if (evnt.attributeName === "style" && evnt.newValue.split("; ")[1].split(":")[0] === "margin-left") {
                ContentScrolled(evnt.newValue.split("; ")[1].split(":")[1]);
            }
        }
    });


    $("div.NewsContent > div.InnerContent").on("click", "div.News > input.delete", function() {
        $.ajax({
            type: "DELETE",
            url: `/res/data/News/${$(this).closest("div.News").attr("contentId")}`,
            success: data => $(`div.News[contentId="${data}"]`).animate({ width: 0 }, 400, function() { $(this).remove() })
        });
    });

    $("div.NewsContent > div.InnerContent").on("click", "div.News > input.save", function() {
        var $parentDiv = $(this).closest('div.News');
        var jsonObj = {
            title: $parentDiv.find("input.title").val(),
            date: $parentDiv.find("input.date").val(),
            content: $parentDiv.find("textarea.content").html()
        };

        $.ajax({
            type: "PUT",
            url: `res/data/News/${$parentDiv.attr("contentId")}`,
            data: jsonObj,
            success: data => console.log("Upload complete")
        });
    });

    $('div.NewsContent > div.InnerContent > input[type="button"].addNews').click(() => {
        let date = new Date();
        let news = {
            date: `${date.getDate()}.${date.getMonth()}.${date.getFullYear()}`,
            title: "Title",
            content: "Content"
        };
        $.ajax({
            method: "POST",
            url: "/res/data/News",
            contentType: "application/json",
            data: news,
            success: data => addNews(data)
        });
    });
    
    $('div.AboutCompContent > div.InnerContent').on('click', 'div.AboutComp > input.save', function() {
        var $parentDiv = $(this).closest('div.AboutComp');
        var jsonObj = {
            title: $parentDiv.find("input.title").val(),
            content: $parentDiv.find("textarea.content").html()
        };
        
        $.ajax({
            type: "PUT",
            url: 'res/data/AboutComp/0',
            data: jsonObj,
            success: data => console.log("Upload complete")
        });
    });
    
    $("div.AboutUsContent > div.InnerContent").on("click", "div.Profile > input.save", function() {
        var $parentDiv = $(this).closest('div.Profile');
        var jsonObj = {
            imageUrl: $parentDiv.find("input.profilePic").val(),
            name: $parentDiv.find("input.name").val(),
            age: $parentDiv.find("input.age").val(),
            desc: $parentDiv.find("textarea.desc").html()
        };

        $.ajax({
            type: "PUT",
            url: `res/data/AboutUs/${$parentDiv.attr("contentId")}`,
            data: jsonObj,
            success: data => console.log("Upload complete")
        });
    });

    $Window.resize(() => {

        if ($Window.outerWidth() > 730) {
            $("nav#TopMenu > ul.NavMenu").css("display", "unset");
        } else $("nav#TopMenu > ul.NavMenu").slideUp();
        SetViewport();
    });

    $("nav#TopMenu > p#HamburgerToggle").click(() => {
        $("nav#TopMenu > ul.NavMenu").slideToggle();
    });

    $("#SuperUberMegaTheMostHiddenAccesEver").click(() => {
        window.location = "";

    });

    $.getJSON("/res/data/News", data => data.forEach(element => addNews(element)));
    $.getJSON("/res/data/AboutComp", data => data.forEach(element => addAboutComp(element)));
    $.getJSON("/res/data/Events", data => data.forEach(element => addEvent(element)));
    $.getJSON("/res/data/AboutUs", data => data.forEach(element => addAboutUs(element)));
});

function addNews(data) {

    $("div.ContentViewport > div.Content > div.NewsContent > div.InnerContent > input.addNews").after(Templates.NewsTemplate(data));
}

function addAboutComp(data) {
    $("div.ContentViewport > div.Content > div.AboutCompContent > div.InnerContent").prepend(Templates.AboutCompTemplate(data));
}

function addEvent(data) {
    $("div.ContentViewport > div.Content > div.EventsContent > div.InnerContent").prepend(Templates.EventsTemplate(data));
}

function addAboutUs(data) {
    $("div.ContentViewport > div.Content > div.AboutUsContent > div.InnerContent").prepend(Templates.AboutUsTemplate(data));
}

function SetViewport() {
    $ContentViewport.css({
        "top": $NavBarHeightReference.outerHeight(),
        "margin-bottom": `${$FooterHeightReference.outerHeight() / $Window.outerHeight() * 100}vh`,
        "height": `${($Window.outerHeight() - $NavBarHeightReference.outerHeight() - $FooterHeightReference.outerHeight()) / $Window.outerHeight() * 100}vh`
    });

    $Content.css("width", 5 * $Window.outerWidth());
}

function ContentScrolled(scroll) {
    clearActive("nav#TopMenu > ul.NavMenu li.NavMenuItem");
    switch (-1 * Math.floor(Number(scroll.replace(/px;/gi, '')) / $Window.outerWidth())) {
        case 0:
            $NewsLink.parent().addClass("active");
            break;
        case 1:
            $AboutCompLink.parent().addClass("active");
            break;
        case 2:
            $EventsLink.parent().addClass("active");
            break;
        case 3:
            $ContactsLink.parent().addClass("active");
            break;
        case 4:
            $AboutUsLink.parent().addClass("active");
            break;
    }
}

function clearActive(selector) {
    $(`${selector}.active`).removeClass("active");
}

class Templates {
    static NewsTemplate(data) {
        return (
            `<div class="News" contentId="${data.id}">
                <input type="text" class="title" value="${data.title}">
                <input type="text" class="date" value="${data.date}">
                <textarea class="content">${data.content}</textarea>
                <input type="button" class="save" value="save">
                <input type="button" class="delete" value="delete">
            </div>`);
    }

    static AboutCompTemplate(data) {
        return (
            `<div class="AboutComp">
                <input type="text" class="title" value="${data.title}">
                <textarea class="content">${data.content}</textarea>
                <input type="button" class="save" value="save">
            </div>`
        );
    }

    static EventsTemplate(data) {
        var linksObject = "";

        if (data.facebook || data.website) {
            linksObject += '<ul class="links">\n';
        }
        if (data.facebook) {
            linksObject += `<li><a href="${data.facebook}" target = "_blank"><img src="res/imgs/facebook_logo.png"></a></li>\n`;
        }
        if (data.website) {
            linksObject += `<li><a href="${data.website}" target = "_blank"><img src="res/imgs/dem_webz.png"></a></li>
            </ul>`;
        }
        return (
            `<div class="Event">
                <h2 class="title">${data.title}</h2>
                <p class="time">${data.time}</p>
                <p class="location">${data.place}</p>` +
            linksObject + `
            </div>`);
    }
    
    static AboutUsTemplate(data) {
        return (
            `<div class="Profile" contentId="${data.id}">
                <input type="text" class="profilePic" value="${data.imageUrl}">
                <input type="text" class="name" value="${data.name}">
                <input type="text" class="age" value="${data.age}">
                <textarea class="desc">${data.desc}</textarea>
                <input type="button" class="save" value="save">
            </div>`
        );
    }
};