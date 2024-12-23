# Veeam Test Task

### Task
Please implement a program that synchronizes two folders: source and
replica. The program should maintain a full, identical copy of source
folder at replica folder. Solve the test task by writing a program in C#.

- Synchronization must be one-way: after the synchronization content of the
  replica folder should be modified to exactly match content of the source
  folder;
- Synchronization should be performed periodically;
- File creation/copying/removal operations should be logged to a file and to the
  console output;
- Folder paths, synchronization interval and log file path should be provided
  using the command line arguments;
- It is undesirable to use third-party libraries that implement folder
  synchronization;
- It is allowed (and recommended) to use external libraries implementing other
  well-known algorithms. For example, there is no point in implementing yet
  another function that calculates MD5 if you need it for the task

### Implementation

This project is a console application to synchronize 2 folders. The project first loops through the replica folder to delete any extra files/directories that are not present in the source folder, and then loops through the source folder to add/update missing folders. To compare if 2 files are identical, the MD5 hashing algorithm is used.

The first implementation that was made **(Class: Synchronizer)** was fast if the replica folder was empty and the algorithm didn't have to compute any MD5 hash. However, when the replica folder is populated with files of equal names (hash calculation is done), the algorithm is very slow.

Some benchmarking demonstrated that for a 4GB folder synchronization with multiple files, computing the hash for every single file took around 35 seconds, compared to the 1-2 seconds to just copy from the source to a new empty folder in the first periodic run.

As I've seen in another repository, one lazy and naive approach would be to delete the replica folder and copy the source every time from scratch. Needless to say, this approach is not realistic, and it is not the solution we are trying to achieve.

To solve the long time to compute the hashes, a new identical but asynchronous class **(Class: SynchronizerAsync)** was implemented, where everything was encapsulated into Tasks. This implementation improved the hash computing from 35 seconds to just 5 seconds at the cost of 100% CPU and memory usage :)

I think this is a good point to stop, as I believe this is a solution that fits well with the scope of this challenge. However, if needed, there are various ways to improve the performance of this project, such as:

- Implementing a new flag to switch between the MD5 hash-based comparison and a LastWriteTime file information comparison, which requires almost no computational effort but does not compare file contents. (This LastWriteTime method is already implemented in the **FileComparator** class)
- Potentially improving the MD5 hash computational cost by dividing the file into chunks and not loading it into memory all at once. (This way also makes it stop the computation prematurely if 2 chunks are different)
- Depending on the scope of usage of this project, checking the size of the file and reading the first and last chunk of bytes of the file might be good enough to compare if 2 files are identical.

### Usage example:

```
dotnet run <sourcePath> <replicaPath> <syncIntervalSeconds> <asyncFlag> <logFilePath>
```

- sourcePath - Source path
- replicaPath - Replica path
- syncIntervalSeconds - Synchronization period/interval in seconds
- asyncFlag - Flag to enable/disable the async computation as mentioned in #Implementation
- logFilePath - Log file path