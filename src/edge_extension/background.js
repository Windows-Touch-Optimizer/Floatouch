const handleRuntimeError = () => {
    const error = chrome.runtime.lastError;
    if (error) {
        throw new Error(error);
    }
};

const safeGetTab = async (tabId) => {
    const tab = await chrome.tabs.get(parseInt(tabId));
    try {
        handleRuntimeError();
        return tab;
    } catch (e){
        console.log('safeGetTab', e.message);
    }
    return undefined;
};

async function tryTabGoBack(tabId) {
	return new Promise((resolve, reject) => {
		chrome.tabs.goBack(null, () => {
			if (chrome.runtime.lastError != undefined) {
				resolve(false);
			} else {
				resolve(true);
			}
		})
	})
}

async function browserGoBack() {
	let currentTabId = tabQueue.pop();
	console.log(currentTabId);
	while (tabQueue.length > 0) {
		let lastTabId = tabQueue[tabQueue.length - 1];
		if (lastTabId == currentTabId) {
			tabQueue.pop();
			continue;
		}
		if (safeGetTab(currentTabId) == undefined) {
			tabQueue.pop();
			continue;
		}
		break;
	}
	if(tabQueue.length>0){
		let lastTabId = tabQueue[tabQueue.length - 1];
		chrome.tabs.update(lastTabId, { active: true, highlighted: true });
		// chrome.windows.update(thisWindowId, { "focused": true });
	}
	chrome.tabs.remove(currentTabId, function () { });
}

const processCommand = async function (command) {
	if (command == "go_back_or_close_tab") {
		if (!await tryTabGoBack()) {
			browserGoBack();
		}
	}
};

chrome.commands.onCommand.addListener(processCommand);

let tabQueue = []

function activeTab(tabId) {
	tabQueue.push(tabId);
}

chrome.tabs.onActivated.addListener(function (tab) {
	let tabId = tab.tabId;
	activeTab(tabId);
});
