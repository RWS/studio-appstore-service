//basic customization
document.documentElement.style.setProperty('--pa-admin-color', GetCookie('BackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-color-hover', GetCookie('BackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-foreground', GetCookie('ForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-fontsize', `${GetCookie('FontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-fontfamily', GetCookie('FontFamily'));

//advanced customization
document.documentElement.style.setProperty('--pa-admin-navbar-fontsize', `${GetCookie('navbarFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-navbar-color', GetCookie('navbarBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-navbar-foreground', GetCookie('navbarForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-navbar-fontfamily', GetCookie('navbarFontFamily'));

document.documentElement.style.setProperty('--pa-admin-modal-fontsize', `${GetCookie('modalFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-modal-color', GetCookie('modalBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-modal-foreground', GetCookie('modalForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-modal-fontfamily', GetCookie('modalFontFamily'));

document.documentElement.style.setProperty('--pa-admin-body-color', GetCookie('bodyBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-body-foreground', GetCookie('bodyForegroundColor'));

document.documentElement.style.setProperty('--pa-admin-select-fontsize', `${GetCookie('selectFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-select-color', GetCookie('selectBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-select-foreground', GetCookie('selectForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-select-fontfamily', GetCookie('selectFontFamily'));

document.documentElement.style.setProperty('--pa-admin-card-fontsize', `${GetCookie('cardFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-card-color', GetCookie('cardBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-card-foreground', GetCookie('cardForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-card-fontfamily', GetCookie('cardFontFamily'));

document.documentElement.style.setProperty('--pa-admin-table-fontsize', `${GetCookie('tableFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-table-color', GetCookie('tableBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-table-foreground', GetCookie('tableForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-table-fontfamily', GetCookie('tableFontFamily'));

document.documentElement.style.setProperty('--pa-admin-input-fontsize', `${GetCookie('inputFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-input-color', GetCookie('inputBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-input-foreground', GetCookie('inputForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-input-fontfamily', GetCookie('inputFontFamily'));

document.documentElement.style.setProperty('--pa-admin-success-fontsize', `${GetCookie('successFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-success-color', GetCookie('successBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-success-foreground', GetCookie('successForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-success-fontfamily', GetCookie('successFontFamily'));
document.documentElement.style.setProperty('--pa-admin-success-color-hover', GetCookie('successBackgroundColor'));

document.documentElement.style.setProperty('--pa-admin-secondary-fontsize', `${GetCookie('secondaryFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-secondary-color', GetCookie('secondaryBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-secondary-foreground', GetCookie('secondaryForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-secondary-fontfamily', GetCookie('secondaryFontFamily'));
document.documentElement.style.setProperty('--pa-admin-secondary-color-hover', GetCookie('secondaryBackgroundColor'));

document.documentElement.style.setProperty('--pa-admin-danger-fontsize', `${GetCookie('dangerFontSize')}px`);
document.documentElement.style.setProperty('--pa-admin-danger-color', GetCookie('dangerBackgroundColor'));
document.documentElement.style.setProperty('--pa-admin-danger-foreground', GetCookie('dangerForegroundColor'));
document.documentElement.style.setProperty('--pa-admin-danger-fontfamily', GetCookie('dangerFontFamily'));
document.documentElement.style.setProperty('--pa-admin-danger-color-hover', GetCookie('dangerBackgroundColor'));

function ChangeFontSize(target) {
    document.documentElement.style.setProperty(`--pa-admin${target}-fontsize`, `${event.target.value}px`);
    document.cookie = `${target.replace('-', '')}FontSize=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function ChangeFontFamily(target) {
    let container = document.querySelector("head");
    let link = container.querySelector('#GoogleFont');
    container.removeChild(link);

    let old = GetCookie('FontFamilies');
    link = document.createElement("link");
    link.href = `https://fonts.googleapis.com/css2?${old}&family=${event.target.value.replace(" ", '+')}&display=swap`
    link.rel = "stylesheet"
    link.id = 'GoogleFont'
    container.append(link);
    document.documentElement.style.setProperty(`--pa-admin${target}-fontfamily`, `${event.target.value}`);
    document.cookie = `${target.replace('-', '')}FontFamily=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`

    let sanitizedCookie = event.target.value.replace(" ", '+');
    if (!old.includes(sanitizedCookie)) {
        document.cookie = `FontFamilies=${old}&family=${sanitizedCookie}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`
    }

    console.log(document.cookie);
}

function ChangeBackground(target) {
    document.documentElement.style.setProperty(`--pa-admin${target}-color`, `${event.target.value}`);
    document.documentElement.style.setProperty(`--pa-admin${target}-color-hover`, `${event.target.value}`);
    document.cookie = `${target.replace('-', '')}BackgroundColor=${event.target.value}; ${target.replace('-', '')}HoverColor=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function ChangeForeground(target) {
    document.documentElement.style.setProperty(`--pa-admin${target}-foreground`, `${event.target.value}`);
    document.cookie = `${target.replace('-', '')}ForegroundColor=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function ChangeLogo() {
    console.log(event.target.value)
    document.querySelector("#LogoImage").src = event.target.value;
    document.querySelector("#LogoImage").width = 75;
    document.cookie = `LogoImage=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function RestorePreferences() {
    var cookies = document.cookie.split(";");

    cookies.forEach(cookie => {
        var name = cookie.indexOf("=") > -1 ? cookie.substring(0, cookie.indexOf("=")) : cookie;
        document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT; Path=/";
    })

    document.cookie = `.AspNet.Consent=yes; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`
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