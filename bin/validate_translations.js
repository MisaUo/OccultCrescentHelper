const fs = require('fs');
const path = require('path');

function load_json(filePath) {
    let content = fs.readFileSync(filePath, 'utf-8');
    if (content.charCodeAt(0) === 0xFEFF) {
        content = content.slice(1);
    }

    return JSON.parse(content);
}

function flatten_keys(obj, prefix = '') {
    let keys = [];
    for (const key in obj) {
        const fullKey = prefix ? `${prefix}.${key}` : key;
        if (obj[key] !== null && typeof obj[key] === 'object' && !Array.isArray(obj[key])) {
            keys = keys.concat(flatten_keys(obj[key], fullKey));
        } else {
            keys.push(fullKey);
        }
    }

    return keys;
}

function get_value_by_path(obj, path) {
    return path.split('.').reduce((o, key) => (o && o[key] !== undefined) ? o[key] : undefined, obj);
}


function check_translations(basePath, baseFile, otherFiles) {
    const baseJson = load_json(path.join(basePath, baseFile));
    const baseKeys = new Set(flatten_keys(baseJson));

    for (const otherFile of otherFiles) {
        const otherJson = load_json(path.join(basePath, otherFile));
        const otherKeys = new Set(flatten_keys(otherJson));

        const missing = [...baseKeys].filter(k => !otherKeys.has(k));
        const excess = [...otherKeys].filter(k => !baseKeys.has(k));

        console.log(`--- Checking ${otherFile} ---`);

        if (missing.length > 0) {
            console.log(`Missing keys (${missing.length}):`);
            missing.sort().forEach(k => {
                const val = get_value_by_path(baseJson, k);
                console.log(`  ${k}: ${JSON.stringify(val)}`);
            });
        } else {
            console.log('No missing keys.');
        }

        if (excess.length > 0) {
            console.log(`Excess keys (${excess.length}):`);
            excess.sort().forEach(k => console.log(`  ${k}`));
        } else {
            console.log('No excess keys.');
        }

        console.log();
    }
}

const basePath = path.join(__dirname, '..', 'Translations');
const baseFile = 'en.json';

const allFiles = fs.readdirSync(basePath).filter(f => f.endsWith('.json'));
const otherFiles = allFiles.filter(f => f !== baseFile);

check_translations(basePath, baseFile, otherFiles);
