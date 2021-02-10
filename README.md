# Autonomous-Vehicle-Simulation

Firstly;
--> You must create environment on Anaconda Prompt : 
    conda create -n my_env python=3.6
    
--> Then, you must activate environment(my_env) : 
    conda activate my_env
    
--> Then, you must install mlagents package(0.21.0). This version must be downloaded. : 
    pip install mlagents==0.21.0 
    
--> And this point is important : numpy version must be 1.19.3 

-->After these steps, the environment is ready for training. 

-->You must download ml-agent 0.15.0 version in your computer. Because, you must add this version in your unity editor. 
   Then, Next, you need to add the CarDrive.yaml file to the config in this downloaded 0.15.0 version. CarDrive.yaml.
   Before the training process, go to the ml-agent folder of which 0.15.0 version is downloaded on anaconda prompt. 

--> Then,Then you can start training with the code below. When you run this code, you will be prompted to run Unity Editor. When you run the Unity environment, the training process will start. : 
    mlagents-learn config/CarDrive.yaml --run-id=myTraining --train
    
    
    #You can find the project presentation above# 
