# Snapshot testing for JSON

A simple library for testing that JSON serializations of classes using JSON.Net doesn't change. The idea
is you take an object, load a snapshot JSON file based on the test name, and make sure the object can be
reconstructed from the snapshot and serializes to JSON that is equivalent. If the snapshot doesn't exist
already, the object is serialized and saved as the snapshot for future tests. Snapshot files are stored
in a directory named "_snapshots" in the same directory as the source code for the test.

By default snapshots are stored in files named for the class and method the snapshotter is called from,
but you can also explicitly name the snapshot. This prevents refactorings of the test from breaking the
link to the snapshot (unless of course you use nameof() to name snapshot).

New snapshots need to be added to source control manually.
