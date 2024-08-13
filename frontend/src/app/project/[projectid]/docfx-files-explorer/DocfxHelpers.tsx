function FullVPath(node: any) {
    if (node.directory === '/') {
        return '/' + node.name;
    } else {
        return node.directory + '/' + node.name;
    }
}

function VPathCombine(path1: string, path2: string) {
    if (!path1.endsWith('/')) {
        path1 += '/'
    }

    return path1 + path2;
}

export { FullVPath, VPathCombine }