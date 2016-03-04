/// <reference path="../typings/main.d.ts" />

var $ContentViewport;
var $Content;
var $Footer;
var $Window;

var $NavBarHeightReference;

var $NewsLink;
var $AboutCompLink;
var $EventsLink;
var $ContactsLink;
var $AboutUsLink;

$(document).ready(() => {

    $ContentViewport = $("div.ContentViewport");
    $Content = $("div.ContentViewport > div.Content");
    $Footer = $("footer");
    $Window = $(window);
    
    $NavBarHeightReference = $("ul.HeaderWrapper");

    $NewsLink = $("#NewsLink");
    $AboutCompLink = $("#AboutCompLink");
    $EventsLink = $("#EventsLink");
    $ContactsLink = $("#ContactsLink");
    $AboutUsLink = $("#AboutUsLink");

    SetViewport();

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

    $EventsLink = $("#EventsLink");
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

    $Window.resize(() => {

        if ($Window.outerWidth() > 730) {
            $("nav#TopMenu > ul.NavMenu").css("display", "unset");
        } else  $("nav#TopMenu > ul.NavMenu").slideUp();
        SetViewport();
    });

    $("nav#TopMenu > p#HamburgerToggle").click(() => {
        $("nav#TopMenu > ul.NavMenu").slideToggle();
    });
    
    $("#SuperUberMegaTheMostHiddenAccesEver").click(() => {
        window.location = "admin";
    });

    $.getJSON("./res/data/News.json", (data, status) => {
        data.forEach(element => {
            $("div.ContentViewport > div.Content > div.NewsContent > div.InnerContent").append(Templates.NewsTemplate(element));
        }, this);
    });

    $.getJSON("./res/data/Events.json", (data, status) => {
        data.forEach(element => {
            $("div.ContentViewport > div.Content > div.EventsContent > div.InnerContent").append(Templates.EventsTemplate(element));
        }, this);
    });
});

function SetViewport() {
    $ContentViewport.css({
        "top": `${$NavBarHeightReference.outerHeight() / $Window.outerHeight() * 100}vh`,
        "margin-bottom": `${$Footer.outerHeight() / $Window.outerHeight() * 100}vh`,
        "height": `${($Window.outerHeight() - $NavBarHeightReference.outerHeight() - $Footer.outerHeight()) / $Window.outerHeight() * 100}vh`
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
            `<div class="News">
                <h2 class="title">${data.title}</h2>
                <p class="date">${data.date}</p>
                <p class="content">${data.content}</p>
            </div>`);
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
};