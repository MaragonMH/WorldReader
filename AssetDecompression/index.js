var fs = require('fs')
var lz4 = require('lz4')

var outDir = "decompressed/"
var inDir = "compressed/"

// Check size of all files in decompressed
fs.readdir(outDir, function(err, filenames) {
    if (err) {
        onError(err);
        return;
    }
    filenames.forEach(function(filename) {
        var input = fs.readFileSync(outDir + filename)
        var length = input.readUInt32LE(6)
        if (length == input.length) return
        input.writeUInt32LE(input.length, 6)
        fs.writeFileSync(outDir + filename, input)
        console.log(`Corrected length of ${filename} from ${length} to ${input.length}`)
    });
})

// Decompress all files in compressed
fs.readdir(inDir, function(err, filenames) {
    if (err) {
        onError(err);
        return;
    }
    filenames.forEach(function(filename) {
        var input = fs.readFileSync(inDir + filename)
        var length = input.readUInt32LE(10)
        var decompressed = Buffer.alloc(length)
        var input2 = input.slice(14)
        lz4.decodeBlock(input2, decompressed)
        var header = Buffer.from("XNBw\x05\x01XXXX");
        header.writeUInt32LE(length + 10, 6)
        var output = Buffer.concat([header, decompressed]);
        fs.writeFileSync(outDir + filename, output)
        console.log(`Decompressed ${filename}`)
    });
});