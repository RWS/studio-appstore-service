const cookieMaxAge = 60 * 60 * 24 * 180;

//basic customization
document.documentElement.style.setProperty('--pa-admin-color', GetCookie('BackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-color-hover', GetCookie('BackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-foreground', GetCookie('ForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-fontsize', `${GetCookie('FontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-fontfamily', GetCookie('FontFamily').replace('+', ' '));

//advanced customization
document.documentElement.style.setProperty('--pa-admin-navbar-fontsize', `${GetCookie('navbarFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-navbar-color', GetCookie('navbarBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-navbar-foreground', GetCookie('navbarForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-navbar-fontfamily', GetCookie('navbarFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-modal-fontsize', `${GetCookie('modalFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-modal-color', GetCookie('modalBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-modal-foreground', GetCookie('modalForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-modal-fontfamily', GetCookie('modalFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-body-color', GetCookie('bodyBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-body-foreground', GetCookie('bodyForegroundColor'));

document.documentElement.style.setProperty('--pa-admin-select-fontsize', `${GetCookie('selectFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-select-color', GetCookie('selectBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-select-foreground', GetCookie('selectForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-select-fontfamily', GetCookie('selectFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-card-fontsize', `${GetCookie('cardFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-card-color', GetCookie('cardBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-card-foreground', GetCookie('cardForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-card-fontfamily', GetCookie('cardFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-table-fontsize', `${GetCookie('tableFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-table-color', GetCookie('tableBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-table-foreground', GetCookie('tableForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-table-fontfamily', GetCookie('tableFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-input-fontsize', `${GetCookie('inputFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-input-color', GetCookie('inputBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-input-foreground', GetCookie('inputForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-input-fontfamily', GetCookie('inputFontFamily').replace('+', ' '));

document.documentElement.style.setProperty('--pa-admin-success-fontsize', `${GetCookie('successFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-success-color', GetCookie('successBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-success-foreground', GetCookie('successForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-success-fontfamily', GetCookie('successFontFamily').replace('+', ' '));
document.documentElement.style.setProperty('--pa-admin-success-color-hover', GetCookie('successBackgroundColor'));

document.documentElement.style.setProperty('--pa-admin-secondary-fontsize', `${GetCookie('secondaryFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-secondary-color', GetCookie('secondaryBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-secondary-foreground', GetCookie('secondaryForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-secondary-fontfamily', GetCookie('secondaryFontFamily').replace('+', ' '));
document.documentElement.style.setProperty('--pa-admin-secondary-color-hover', GetCookie('secondaryBackgroundColor'));

document.documentElement.style.setProperty('--pa-admin-danger-fontsize', `${GetCookie('dangerFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-danger-color', GetCookie('dangerBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-danger-foreground', GetCookie('dangerForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-danger-fontfamily', GetCookie('dangerFontFamily').replace('+', ' '));
document.documentElement.style.setProperty('--pa-admin-danger-color-hover', GetCookie('dangerBackgroundColor'));

function ChangeFontSize(target) {
    event.target.dataset.override = "False";
    document.documentElement.style.setProperty(`--pa-admin${target}-fontsize`, `${event.target.value}px`);
    document.cookie = `${target.replace('-', '')}FontSize=${event.target.value}; max-age=${cookieMaxAge}; Path=/;`;
}

function ChangeFontFamily(target) {
    event.target.parentElement.previousElementSibling.dataset.override = "false";
    event.target.parentElement.previousElementSibling.innerText = event.target.innerText;
    document.documentElement.style.setProperty(`--pa-admin${target}-fontfamily`, `${event.target.innerText}`);
    document.cookie = `${target.replace('-', '')}FontFamily=${event.target.innerText.replace(' ', '+')}; max-age=${cookieMaxAge}; Path=/;`
}

function ChangeBackground(target) {
    event.target.dataset.override = "false";
    document.documentElement.style.setProperty(`--pa-admin${target}-color`, `${event.target.value}`);
    document.documentElement.style.setProperty(`--pa-admin${target}-color-hover`, `${event.target.value}`);
    document.cookie = `${target.replace('-', '')}BackgroundColor=${event.target.value}; ${target.replace('-', '')}HoverColor=${event.target.value}; max-age=${cookieMaxAge}; Path=/;`;
}

function ChangeForeground(target) {
    event.target.dataset.override = "false";
    document.documentElement.style.setProperty(`--pa-admin${target}-foreground`, `${event.target.value}`);
    document.cookie = `${target.replace('-', '')}ForegroundColor=${event.target.value}; max-age=${cookieMaxAge}; Path=/;`;
}

function ChangeLogo() {
    console.log(event.target.value)
    document.querySelector("#LogoImage").src = event.target.value;
    document.querySelector("#LogoImage").width = 75;
    document.cookie = `LogoImage=${event.target.value}; max-age=${cookieMaxAge}; Path=/;`;
}

function RestorePreferences() {
    var cookies = document.cookie.split(";");

    cookies.forEach(cookie => {
        var name = cookie.indexOf("=") > -1 ? cookie.substring(0, cookie.indexOf("=")) : cookie;
        document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT; Path=/";
    })

    document.cookie = `.AspNet.Consent=yes; Path=/;`
    window.location.reload();
}

function GetCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        return parts.pop().split(';').shift();
    }

    return '';
}

function ApplyFonts() {
    if (event.type == "click") {
        Array.from(event.target.nextElementSibling.querySelectorAll('li')).slice(0, 12).forEach(item => {
            if (item.fontFamily == undefined) {
                item.style.fontFamily = item.innerText;
            }
        })
    }

    if (event.type == "scroll") {
        let position = event.target.scrollTop;
        Array.from(event.target.querySelectorAll('li')).slice(Math.round((position + 200) / 33), Math.round((position + 400) / 33)).forEach(item => {
            if (item.fontFamily == undefined) {
                item.style.fontFamily = item.innerText;
            }
        })
    }
}