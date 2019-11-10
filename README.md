# Snapshot testing for JSON

A simple library for testing that JSON serializations of classes using JSON.Net doesn't change. The idea is you take an object, load a snapshot JSON file based on the test name, and make sure the object can be reconstructed from the snapshot and serializes to JSON that is equivalent. If the snapshot doesn't exist already, the object is serialized and saved as the snapshot for future tests. Snapshot files are stored in a directory named "_snapshots" in the same directory as the source code for the test.

New snapshots need to be added to source control manually.
